using System.Reflection;

using DSharpPlus.Entities;
using DSharpPlus;

namespace DSharpPlus.SlashCommands.Enums
{
    public static class ApplicationCommandOptionTypeExtensions
    {
        public static ApplicationCommandOptionType? GetOptionType(ParameterInfo parameter)
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
                return null;
        }
    }
}
