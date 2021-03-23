using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Entities
{
    public class SlashSubcommand
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public MethodInfo ExecutionMethod { get; init; }
        public BaseSlashCommandModule BaseCommand { get; init; }

        public SlashSubcommand(string name, string desc, MethodInfo method, BaseSlashCommandModule commandInstance)
        {
            Name = name;
            Description = desc;
            ExecutionMethod = method;
            BaseCommand = commandInstance;
        }

        public async Task ExecuteCommand(BaseDiscordClient c, ulong? guildId, params object[] args)
        {
            try
            {
                var parsedArgs = await ParseArguments(c, guildId, args);
                ExecutionMethod.Invoke(BaseCommand, parsedArgs);
            }
            catch (Exception ex)
            {
                // TODO Loger here
            }
        }

        private async Task<object[]> ParseArguments(BaseDiscordClient c, ulong? guildId, object[] args)
        {
            var parsedArgs = new object[args.Length];
            var parameters = ExecutionMethod.GetParameters();

            for(int i = 0; i < args.Length; i++)
            {
                var param = parameters[i];

                if (param.ParameterType.IsEnum)
                {
                    var e = ParseEnum(args[i], param);

                    if (e is null)
                        throw new ArgumentNullException(param.Name, "Failed to parse Discord result to enum value.");

                    parsedArgs[i] = e;
                }
                else if (param.ParameterType == typeof(DiscordUser))
                {
                    var u = await ParseUser(args[i], c);

                    if (u is null)
                        throw new ArgumentNullException(param.Name, "Failed to parse Discord result to DiscordUser value");

                    parsedArgs[i] = u;
                }
                else if (param.ParameterType == typeof(DiscordChannel))
                {
                    var chan = await ParseChannel(args[i], c);

                    if (chan is null)
                        throw new ArgumentNullException(param.Name, "Failed to parse Discord result to DiscordChannel value");

                    parsedArgs[i] = chan;
                }
                else if (param.ParameterType == typeof(DiscordRole) && guildId is not null)
                {
                    var r = await ParseRole(args[i], c, guildId.Value);

                    if (r is null)
                        throw new ArgumentNullException(param.Name, "Failed to parse Discord result to DisocrdRole value");

                    parsedArgs[i] = r;
                }
                else
                {
                    try
                    {
                        object parsed = Convert.ChangeType(args[i], param.ParameterType);
                        parsedArgs[i] = parsed;
                    }
                    catch(Exception ex)
                    {
                        // TODO: Log this
                        // Failed basic conversion.
                    }
                }
            }

            return parsedArgs;
        }

        private static object? ParseEnum(object arg, ParameterInfo info)
        {
            try
            {
                var e = Enum.ToObject(info.ParameterType, arg);
                return e;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<DiscordUser?> ParseUser(object arg, BaseDiscordClient client)
        {
            try
            {
                ulong argVal = Convert.ToUInt64(arg);

                return client switch
                {
                    DiscordClient discord => await discord.GetUserAsync(argVal),
                    DiscordRestClient rest => await rest.GetUserAsync(argVal),
                    _ => null,
                };
            }
            catch
            {
                // TODO: Logger here.

                return null;
            }
        }

        private static async Task<DiscordChannel?> ParseChannel(object arg, BaseDiscordClient client)
        {
            try
            {
                ulong argVal = Convert.ToUInt64(arg);
                return client switch
                {
                    DiscordClient discord => await discord.GetChannelAsync(argVal),
                    DiscordRestClient rest => await rest.GetChannelAsync(argVal),
                    _ => null,
                };
            }
            catch
            {
                // TODO: Logger here.

                return null;
            }
        }

        private static async Task<DiscordRole?> ParseRole(object arg, BaseDiscordClient client, ulong guildId)
        {
            try
            {
                ulong argVal = Convert.ToUInt64(arg);

                switch (client)
                {
                    case DiscordClient discord:
                        var guild = await discord.GetGuildAsync(guildId);
                        return guild.GetRole(argVal);
                    case DiscordRestClient rest:
                        var roles = await rest.GetGuildRolesAsync(guildId);
                        return roles.FirstOrDefault(x => x.Id == argVal);
                }

                return null;
            }
            catch
            {
                // TODO: Logger here

                return null;
            }
        }
    }
}
