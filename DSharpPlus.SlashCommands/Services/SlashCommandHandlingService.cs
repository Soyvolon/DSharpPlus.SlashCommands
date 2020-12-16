using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Enums;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.SlashCommands.Services
{
    public class SlashCommandHandlingService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;
        private readonly HttpClient _client;
        private ulong BotId { get; set; }
        private string Token { get; set; }

        private ConcurrentDictionary<string, SlashCommand> Commands { get; set; }

        public SlashCommandHandlingService(IServiceProvider services, ILogger<SlashCommandHandlingService> logger, HttpClient http)
        {
            _logger = logger;
            _services = services;
            _client = http;

            Commands = new();
        }

        public void Start(string botToken, ulong clientId)
        {
            Token = botToken;
            BotId = clientId;

            LoadCommandTree();
            VerifyCommandState();
        }

        /// <summary>
        /// Loads the commands from the assembly.
        /// </summary>
        // TODO: Pass in Assembly values to specify where to look for commands.
        private void LoadCommandTree()
        {
            _logger.LogInformation("Building Slash Command Objects");
            // Get the base command class type...
            var cmdType = typeof(SlashCommandBase);
            // ... and all the methods in it...
            var commandMethods = cmdType.GetMethods().ToList();

            // ... and then all the classes in this assembly ...
            var types = Assembly.GetAssembly(typeof(SlashCommandBase))?.GetTypes();
            // ... and if the types are not null ...
            if(types is not null)
            {
                // ... then for each type that is a subclass of SlashCommandBase ...
                foreach (var t in types.Where(x => x.IsSubclassOf(cmdType)))
                {
                    // ... add its methods as command methods.
                    commandMethods.AddRange(t.GetMethods());
                }
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
                        var slashCmdClass = cmd.DeclaringType;
                        // ... and the SlashCommand attribute for that class...
                        SlashCommandAttribute? slashAttr;
                        if(slashCmdClass is not null
                            && (slashAttr = slashCmdClass.GetCustomAttribute<SlashCommandAttribute>(false)) is not null)
                        { //... if it is a slash command, get or add the SlashCommand for the command ...
                            if (!commands.ContainsKey(slashAttr.Name))
                                commands.Add(slashAttr.Name, new SlashCommand(slashAttr.Name, Array.Empty<SlashSubcommandGroup>()));

                            if(commands.TryGetValue(slashAttr.Name, out var slashCommand))
                            { //... and then make sure it has subcommands ...
                                if (slashCommand.Subcommands is null)
                                    throw new Exception("Can't add a subcommand to a Slash Command without subcommands.");
                                // ... then get or add the subcommand for this command method ...
                                if(!slashCommand.Subcommands.ContainsKey(subGroupAttr.Name))
                                    slashCommand.Subcommands.Add(subGroupAttr.Name, new SlashSubcommandGroup(subGroupAttr.Name));

                                if (slashCommand.Subcommands.TryGetValue(subGroupAttr.Name, out var slashSubcommandGroup))
                                { //... and ensure the command does not already exsist ...
                                    if (slashSubcommandGroup.Commands.ContainsKey(attr.Name))
                                        throw new Exception("Can't have two subcommands of the same name!");

                                    // ... then build an instance of the command ...
                                    // TODO: Actually make this dependency injection isntead of just passing the
                                    // services into the base slash command class.
                                    var instance = Activator.CreateInstance(slashCmdClass, _services);
                                    // ... verify it was made correctly ...
                                    if (instance is null)
                                        throw new Exception("Failed to build command class instance");
                                    // ... and save the subcommand.
                                    slashSubcommandGroup.Commands.Add(attr.Name,
                                        new SlashSubcommand(attr.Name, cmd,
                                            (SlashCommandBase)instance
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
                            throw new Exception("A Subcommand Group is required to be inside a class marked with a SlashCommand attribute");
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

            _logger.LogInformation("Added subcommand groupings, reading non-subcommand methods...");

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
                        || !cmd.DeclaringType.IsSubclassOf(typeof(SlashCommandBase)))
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
                            new SlashSubcommand(
                                attr.Name,
                                cmd,
                                (SlashCommandBase)instance
                            )
                        ));
                }
                // ... otherwise, ignore the method.
            }

            _logger.LogInformation("Commands from source loaded.");

            Commands = new(commands);
        }

        private async void VerifyCommandState()
        {
            _logger.LogInformation("Attempting to read previous slash command state.");

            string json;
            // (Use sectioned using statements here beacuse we will write to the JSON file later in this method)
            using (FileStream fs = new($"sccfg_{BotId}.json", FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new(fs))
                {
                    json = await sr.ReadToEndAsync();
                }
            }


        }

        private async Task RemoveOldCommands(HashSet<SlashCommandConfiguration> toRemove)
        {
            foreach (var scfg in toRemove)
            {
                HttpRequestMessage msg = new();
                msg.Headers.Authorization = new("Bot", Token);
                msg.Method = HttpMethod.Delete;

                if (scfg.GuildId is not null)
                {
                    msg.RequestUri = new Uri($"https://discord.com/applications/{BotId}/guilds/{scfg.GuildId}/commands/{scfg.CommandId}");
                }
                else
                {
                    msg.RequestUri = new Uri($"https://discord.com/applications/{BotId}/commands/{scfg.CommandId}");
                }

                var response = await _client.SendAsync(msg);

                if(response.IsSuccessStatusCode)
                {
                    Commands.TryRemove(scfg.Name, out _);
                }
            }
        }

        private async Task UpdateOrAddCommand(HashSet<Tuple<string, MethodInfo>> toUpdate)
        {
            foreach(var update in toUpdate)
            {
                HttpRequestMessage msg = new();
                msg.Headers.Authorization = new("Bot", Token);
                msg.Method = HttpMethod.Post;

                SlashCommandAttribute? attr;
                if ((attr = update.Item2.GetCustomAttribute<SlashCommandAttribute>()) is not null)
                {
                    if (attr.GuildId is not null)
                    {
                        msg.RequestUri = new Uri($"https://discord.com/applications/{BotId}/guilds/{attr.GuildId}/commands");
                    }
                    else
                    {
                        msg.RequestUri = new Uri($"https://discord.com/applications/{BotId}/commands");
                    }
                }
                else throw new Exception("Failed to get SlashCommandAttribute on update/add");

                string desc = update.Item2.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";

                var ps = update.Item2.GetParameters();

                List<ApplicationCommandOption> options = new();

                foreach (var p in ps)
                {
                    var op = new ApplicationCommandOption()
                    {
                        Name = p.Name ?? "unkown",
                        Description = p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
                        Type = GetOptionType(p) ?? throw new Exception("Failed to get valid option")
                    };
                }

                if (options.Count > 10) throw new Exception("Options must be less than 10 items long");

                var cmd = new ApplicationCommand()
                {
                    ApplicationId = BotId,
                    Description = desc,
                    Name = attr.Name,
                    Options = options.ToArray()
                };

                msg.Content = JsonContent.Create(cmd);


            }
        }

        private static ApplicationCommandOptionType? GetOptionType(ParameterInfo parameter)
        {
            if (parameter.ParameterType == typeof(string))
                return ApplicationCommandOptionType.String;
            else if (parameter.ParameterType == typeof(int))
                return ApplicationCommandOptionType.Integer;
            else if (parameter.ParameterType == typeof(bool))
                return ApplicationCommandOptionType.Boolean;
            else if (parameter.ParameterType == typeof(DiscordUser))
                return ApplicationCommandOptionType.User;
            else if (parameter.ParameterType == typeof(DiscordChannel))
                return ApplicationCommandOptionType.Channel;
            else if (parameter.ParameterType == typeof(DiscordRole))
                return ApplicationCommandOptionType.Role;
            else
                throw new Exception("Invalid paramter type for slash commands");
        }

        private async Task RegisterCommand(MethodInfo method, SlashCommandAttribute attr)
        {

        }
    }
}
