using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Enums;
using Microsoft.Extensions.Logging;
using DSharpPlus;

using Newtonsoft.Json;

namespace DSharpPlus.SlashCommands.Services
{
    public class SlashCommandHandlingService
    {
        public bool Started { get; private set; }

        private readonly ILogger _logger;
        private readonly IServiceProvider _services;
        private readonly HttpClient _client;
        private ulong BotId { get; set; }
        private string Token { get; set; }
        private string ConfigPath { get
            {
                return $"sccfg_{BotId}.json";
            }
        }

        private ConcurrentDictionary<string, SlashCommand> Commands { get; set; }
        private List<Assembly> Assemblies { get; set; }

        private ConcurrentDictionary<DiscordInteraction, Tuple<Task, CancellationTokenSource>> RunningInteractions;


        /// <summary>
        /// Create a new Slash Command Service. Best used by adding it into a service collection, then pulling it once and running start. Or,
        /// when it is used, verify it is Started and run Start if it is not.
        /// </summary>
        /// <param name="services">Services for DI (which is kinda not really implemented)</param>
        /// <param name="http">HTTP Client for making web requests to the Discord API</param>
        /// <param name="logger">A Logger for logging what this service does.</param>
        public SlashCommandHandlingService(IServiceProvider services, HttpClient http, ILogger logger)
        {
            _logger = logger;
            _services = services;
            _client = http;

            Commands = new();
            Assemblies = new();
            RunningInteractions = new();
            Started = false;
        }

        /// <summary>
        /// Add an assembly to register commands from.
        /// </summary>
        /// <param name="assembly">Assembly to get commands from.</param>
        public void WithCommandAssembly(Assembly assembly)
        {
            Assemblies.Add(assembly);
        }

        /// <summary>
        /// Register the commands and allow the service to handle commands.
        /// </summary>
        /// <param name="botToken">Bot token for authentication</param>
        /// <param name="clientId">Bot Client ID, used for storing command state locally.</param>
        public async Task StartAsync(string botToken, ulong clientId)
        {
            Token = botToken;
            BotId = clientId;

            LoadCommandTree();
            await BulkUpdateCommands();
        }

        public Task HandleInteraction(BaseDiscordClient discord, DiscordInteraction interact, DiscordSlashClient c)
        {
            // This should not get here, but check just in case.
            if (interact.Type == InteractionType.Ping) return Task.CompletedTask;
            // Create a cancellation token for the event in which it is needed.
            var cancelSource = new CancellationTokenSource();
            // Store the command task in a ConcurrentDictionary and continue with execution to not hodlup the webhook response.
            RunningInteractions[interact] = new(
                Task.Run(async () => await ExecuteInteraction(discord, interact, c, cancelSource.Token)),
                cancelSource);

            return Task.CompletedTask;
        }

        private async Task ExecuteInteraction(BaseDiscordClient discord, DiscordInteraction interact, DiscordSlashClient c, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (interact.Data is null)
                    throw new Exception("Interact object has no command data.");

                if(Commands.TryGetValue(interact.Data.Name, out var cmd))
                { // TODO: Check how subcommands are returned.
                    // TODO: Do argument parsing.

                    var context = new InteractionContext(c, interact);

                    if(interact.Data.Options is not null)
                    {
                        var args = await GetRawArguments(interact.Data.Options);

                        cmd.ExecuteCommand(discord, context, args);
                    }
                    else
                    {
                        cmd.ExecuteCommand(discord, context);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Interaction Handler failed");
            }
            finally
            {
                RunningInteractions.TryRemove(interact, out _);
            }
        }

        private async Task<object[]> GetRawArguments(IEnumerable<DiscordInteractionDataOption> options)
        {
            if(options.FirstOrDefault()?.Options is not null)
            {
                return await GetRawArguments(options.First().Options);
            }
            else
            {
                var args = new List<object>();

                foreach(var val in options)
                {
                    if (val.Value is null)
                        continue;

                    args.Add(val.Value);
                }

                return args.ToArray();
            }
        }

        #region Command Registration
        /// <summary>
        /// Loads the commands from the assembly.
        /// </summary>
        // TODO: Pass in Assembly values to specify where to look for commands.
        private void LoadCommandTree()
        {
            _logger.LogInformation("Building Slash Command Objects ...");
            // Get the base command class type...
            var cmdType = typeof(BaseSlashCommandModule);
            // ... and all the methods in it...
            var commandMethods = cmdType.GetMethods().ToList();

            // ... and then all the classes in the provided assemblies ...
            List<Type> types = new();
            foreach(var a in Assemblies)
            {
                // ... and add the types from that aseembly that are subclasses of the command type.
                types.AddRange(a.GetTypes().Where(x => x.IsSubclassOf(cmdType)));
            }

            // ... then for each type that is a subclass of SlashCommandBase ...
            foreach (var t in types)
            {
                // ... add its methods as command methods.
                commandMethods.AddRange(t.GetMethods());
            }

            //... and create a list for methods that are not subcommands...
            List<MethodInfo> nonSubcommandCommands = new();
            //... and a dict for all registered commands ...
            Dictionary<string, SlashCommand> commands = new();
            // ... and for every command ...
            foreach (var cmd in commandMethods)
            {
                // ... try and get the SlashSubommandAttribute for it...
                // (we will check for methods with just the SlashCommandAttribute later)
                SlashSubcommandAttribute? attr;
                if((attr = cmd.GetCustomAttribute<SlashSubcommandAttribute>(false)) is not null)
                { //... if it is a subcommand, get the class that the subcommand is in...
                    var subGroupClass = cmd.DeclaringType;
                    // ... and the SubcommandGroup attribute for that class ...
                    SlashSubcommandGroupAttribute? subGroupAttr;
                    if(subGroupClass is not null 
                        && (subGroupAttr = subGroupClass.GetCustomAttribute<SlashSubcommandGroupAttribute>(false)) is not null)
                    { //... if it is a subcommand group, get the class the subcommand group is in...
                        var slashCmdClass = subGroupClass.BaseType;
                        // ... and the SlashCommand attribute for that class...
                        SlashCommandAttribute? slashAttr;
                        if(slashCmdClass is not null
                            && (slashAttr = slashCmdClass.GetCustomAttribute<SlashCommandAttribute>(false)) is not null)
                        { //... if it is a slash command, get or add the SlashCommand for the command ...
                            if (!commands.ContainsKey(slashAttr.Name))
                                commands.Add(slashAttr.Name, new SlashCommand(slashAttr.Name, 
                                    slashAttr.Version, 
                                    Array.Empty<SlashSubcommandGroup>(),
                                    slashAttr.GuildId));

                            if(commands.TryGetValue(slashAttr.Name, out var slashCommand))
                            { //... and then make sure it has subcommands ...
                                if (slashCommand.Subcommands is null)
                                    throw new Exception("Can't add a subcommand to a Slash Command without subcommands.");
                                // ... then get or add the subcommand for this command method ...
                                if(!slashCommand.Subcommands.ContainsKey(subGroupAttr.Name))
                                    slashCommand.Subcommands.Add(subGroupAttr.Name,
                                        new SlashSubcommandGroup(subGroupAttr.Name,
                                        subGroupClass.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "n/a"));

                                if (slashCommand.Subcommands.TryGetValue(subGroupAttr.Name, out var slashSubcommandGroup))
                                { //... and ensure the command does not already exsist ...
                                    if (slashSubcommandGroup.Commands.ContainsKey(attr.Name))
                                        throw new Exception("Can't have two subcommands of the same name!");

                                    // ... then build an instance of the command ...
                                    // TODO: Actually make this dependency injection isntead of just passing the
                                    // services into the base slash command class.
                                    var instance = Activator.CreateInstance(subGroupClass, _services);
                                    // ... verify it was made correctly ...
                                    if (instance is null)
                                        throw new Exception("Failed to build command class instance");
                                    // ... and save the subcommand.
                                    slashSubcommandGroup.Commands.Add(attr.Name,
                                        new SlashSubcommand(attr.Name,
                                            desc: cmd.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "n/a",
                                            cmd,
                                            (BaseSlashCommandModule)instance
                                            )
                                        );
                                }
                                else
                                { //... otherwise tell the user no subcommand was found.
                                    throw new Exception("Failed to get a subcommand grouping!");
                                }
                            }
                            else
                            { // ... otherwise tell the user no slash command was found.
                                throw new Exception("Failed to get Slash Command");
                            }
                        }
                        else
                        { // ... otherwise tell the user a subcommand group needs to be in a slash command class
                            throw new Exception("A Subcommand Group is required to be a child of a class marked with a SlashCommand attribute");
                        }
                    }
                    else
                    { // ... otherwise tell the user a subcommand needs to be in a subcommand group
                        throw new Exception("A Subcommand is required to be inside a class marked with a SubcommandGroup attribute");
                    }
                }
                else
                { // ... if there was no subcommand attribute, store if for checking
                    // if the method is a non-subcommand command.
                    nonSubcommandCommands.Add(cmd);
                }
            }

            _logger.LogInformation("... Added subcommand groupings, reading non-subcommand methods ...");

            // ... take the non-subcommand list we built in the last loop ...
            foreach(var cmd in nonSubcommandCommands)
            {
                // ... and see if any of the methods have a SlashCommand attribute ...
                SlashCommandAttribute? attr;
                if((attr = cmd.GetCustomAttribute<SlashCommandAttribute>(false)) is not null)
                {
                    // ... if they do, make sure it is not also a subcommand ...
                    if (cmd.GetCustomAttribute<SlashSubcommandAttribute>(false) is not null)
                        throw new Exception("A command can not be a subcommand as well.");
                    // ... and that it does not already exsist ...
                    if (commands.ContainsKey(attr.Name))
                        throw new Exception($"A command with the name {attr.Name} already exsists.");
                    // ... and that it has a declaring type AND that type is a subclass of SlashCommandBase ...
                    if (cmd.DeclaringType is null 
                        || !cmd.DeclaringType.IsSubclassOf(typeof(BaseSlashCommandModule)))
                        throw new Exception("A SlashCommand method needs to be in a class.");
                    // ... then build and instance of the class ...
                    // TODO: Actually make this dependency injection isntead of just passing the
                    // services into the base slash command class.
                    var instance = Activator.CreateInstance(cmd.DeclaringType, _services);
                    // ... verify the instance is not null ...
                    if (instance is null)
                        throw new Exception("Failed to build command class instance");
                    // ... and the full comamnd object to the command dict.
                    commands.Add(attr.Name,
                        new SlashCommand(attr.Name,
                            attr.Version,
                            new SlashSubcommand(
                                attr.Name,
                                desc: cmd.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "n/a",
                                cmd,
                                (BaseSlashCommandModule)instance
                            ),
                            attr.GuildId
                        ));
                }
                // ... otherwise, ignore the method.
            }

            _logger.LogInformation("... Commands from source loaded.");

            Commands = new(commands);
        }

        private async Task BulkUpdateCommands()
        {
            List<DiscordApplicationCommand> commandList = new();
            foreach (SlashCommand c in Commands.Values)
                commandList.Add(BuildApplicationCommand(c));

            var json = JsonConvert.SerializeObject(commandList, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            HttpRequestMessage msg = new();
            msg.Method = HttpMethod.Put;
            msg.Content = new StringContent(json);
            msg.Content.Headers.ContentType = new("application/json");
            msg.RequestUri = new Uri($"https://discord.com/api/applications/{BotId}/commands");

            _logger.LogInformation("Executing command update");

            var response = await _client.SendAsync(msg);

            if(response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();

                var commands = JsonConvert.DeserializeObject<List<DiscordApplicationCommand>>(responseJson);

                foreach(DiscordApplicationCommand newCommand in commands)
                {
                    // ... and the old command data ...
                    var oldCommand = Commands[newCommand.Name];
                    // ... then update the old command with the new command.
                    if (newCommand is not null && oldCommand is not null)
                    {
                        oldCommand.ApplicationId = newCommand.ApplicationId;
                        oldCommand.CommandId = newCommand.Id;
                    }
                }

                _logger.LogInformation("Command update complete.");
            }
            else
            {
                _logger.LogCritical($"Command update failed. {response.ReasonPhrase}");
            }
        }

        private DiscordApplicationCommand BuildApplicationCommand(SlashCommand cmd)
        {
            // Create the command builder object ...
            var builder = new ApplicationCommandBuilder()
                .WithName(cmd.Name) // ... set the command name ...
                .WithDescription(cmd.Description); // ... and its description ...
            // ... then, if it has subcommands ...
            if(cmd.Subcommands is not null)
            { // ... for every subcommand, add the option for it.
                foreach (var sub in cmd.Subcommands)
                    builder.AddOption(GetSubcommandOption(sub.Value));
            }
            else if(cmd.Command is not null)
            { // ... otherwise directly add the paramater options for this command ...
                var parameters = cmd.Command.ExecutionMethod.GetParameters();
                if (parameters.Length > 1)
                { // ... if there are any other paramaters besides the Interaction.
                    builder.Options = GetCommandAttributeOptions(parameters[1..]);
                } // ... otherwise we leave this as null.
            }
            // ... then build and return the command.
            return builder.Build();
        }

        private ApplicationCommandOptionBuilder GetSubcommandOption(SlashSubcommandGroup commandGroup)
        { // ... propogate the subcommand group ...
            var builder = new ApplicationCommandOptionBuilder()
                .WithName(commandGroup.Name) // ... with a name ...
                .WithDescription(commandGroup.Description) // ... description ...
                .WithType(ApplicationCommandOptionType.SubCommandGroup); // ... a group type ...
            // ... then load the commands into the group ...
            foreach (var cmd in commandGroup.Commands)
                builder.AddOption(GetSubcommandOption(cmd.Value));
            // ... and return the command option builder.
            return builder;
        }

        private ApplicationCommandOptionBuilder GetSubcommandOption(SlashSubcommand cmd)
        { // ... propogate the subcommand ...
            var builder = new ApplicationCommandOptionBuilder()
                .WithName(cmd.Name) // ... with a name ...
                .WithDescription(cmd.Description) // ... its description ...
                .WithType(ApplicationCommandOptionType.SubCommand); // ... the subcommand type ...
            // ... then get its parameter ...
            var parameters = cmd.ExecutionMethod.GetParameters();
            // ... and if there is more than just the Interaction parameter ...
            if (parameters.Length > 1)
            { // ... load the parmeter options in.
                builder.Options = GetCommandAttributeOptions(parameters[1..]);
            }
            // ... then return the builder.
            return builder;
        }

        private List<ApplicationCommandOptionBuilder> GetCommandAttributeOptions(ParameterInfo[] parameters)
        { // ... create a list for all the command options ...
            List<ApplicationCommandOptionBuilder> builders = new();
            // ... and for each parameter ...
            foreach(var param in parameters)
            { // ... propograte the inital command options ...
                var b = new ApplicationCommandOptionBuilder()
                    .WithName(param.Name ?? "noname") // ... with a name ...
                    .WithDescription(param.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "n/a") // ... a description ...
                    .IsRequired(!param.HasDefaultValue) // ... if it is required or not ...
                    .IsDefault(param.GetCustomAttribute<DefaultParameterAttribute>() is not null); // ... if it is the default ...
                    // ... then see if it is an enum ...
                if(param.ParameterType.IsEnum)
                { //... and load it in as an int with choices ...
                    b.WithType(ApplicationCommandOptionType.Integer)
                        .WithChoices(param.ParameterType);
                }
                else
                { // ... or as a regualr parameter ...
                    var type = ApplicationCommandOptionTypeExtensions.GetOptionType(param);
                    if (type is null) // ... and get the type and verify it is valid ...
                        throw new Exception("Invalid paramater type of slash command.");
                    // ... and add the type.
                    b.WithType(type.Value);
                }
                // ... then store it for the return value.
                builders.Add(b);
            }
            // ... then return the builders list.
            return builders;
        }
        #endregion
    }
}
