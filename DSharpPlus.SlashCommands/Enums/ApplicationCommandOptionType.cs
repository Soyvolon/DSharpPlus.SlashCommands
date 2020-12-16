using System;
using System.Reflection;

using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Enums
{
    public enum ApplicationCommandOptionType
    {
        SubCommand = 1,
        SubCommandGroup = 2,
        String = 3,
        Integer = 4,
        Boolean = 5,
        User = 6,
        Channel = 7,
        Role = 8
    }

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
