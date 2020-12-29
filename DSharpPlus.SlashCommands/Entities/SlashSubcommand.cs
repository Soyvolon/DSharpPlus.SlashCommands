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

        public async Task ExecuteCommand(BaseDiscordClient c, ulong guildId, params object[] args)
        {
            var parsedArgs = await ParseArguments(c, guildId, args);
            ExecutionMethod.Invoke(BaseCommand, parsedArgs);
        }

        private async Task<object[]> ParseArguments(BaseDiscordClient c, ulong guildId, object[] args)
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
                else if(param.ParameterType == typeof(DiscordUser))
                {
                    var u = await ParseUser(args[i], c);

                    if (u is null)
                        throw new ArgumentNullException(param.Name, "Failed to parse Discord result to DiscordUser value");

                    parsedArgs[i] = u;
                }
                else if(param.ParameterType == typeof(DiscordChannel))
                {
                    var chan = await ParseChannel(args[i], c);

                    if (chan is null)
                        throw new ArgumentNullException(param.Name, "Failed to parse Discord result to DiscordChannel value");

                    parsedArgs[i] = chan;
                }
                else if(param.ParameterType == typeof(DiscordRole))
                {
                    var r = await ParseRole(args[i], c, guildId);

                    if (r is null)
                        throw new ArgumentNullException(param.Name, "Failed to parse Discord result to DisocrdRole value");

                    parsedArgs[i] = r;
                }
                else
                    parsedArgs[i] = args[i];
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
            switch(client)
            {
                case DiscordClient discord:
                    return await discord.GetUserAsync((ulong)arg);
                case DiscordRestClient rest:
                    return await rest.GetUserAsync((ulong)arg);
            }

            return null;
        }

        private static async Task<DiscordChannel?> ParseChannel(object arg, BaseDiscordClient client)
        {
            switch (client)
            {
                case DiscordClient discord:
                    return await discord.GetChannelAsync((ulong)arg);
                case DiscordRestClient rest:
                    return await rest.GetChannelAsync((ulong)arg);
            }

            return null;
        }

        private static async Task<DiscordRole?> ParseRole(object arg, BaseDiscordClient client, ulong guildId)
        {
            switch (client)
            {
                case DiscordClient discord:
                    var guild = await discord.GetGuildAsync(guildId);
                    return guild.GetRole((ulong)arg);
                case DiscordRestClient rest:
                    var roles = await rest.GetGuildRolesAsync(guildId);
                    ulong argId = (ulong)arg;
                    return roles.FirstOrDefault(x => x.Id == argId);
            }

            return null;
        }
    }
}
