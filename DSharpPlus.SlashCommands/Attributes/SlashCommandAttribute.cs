using System;

namespace DSharpPlus.SlashCommands.Attributes
{
    /// <summary>
    /// Used to designate a class as a slash command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SlashCommandAttribute : Attribute
    {
        public string Name { get; init; }
        public ulong? GuildId { get; init; }

        public SlashCommandAttribute(string name, ulong guildId = 0)
        {
            Name = name;
            GuildId = guildId == 0 ? null : guildId;
        }
    }
}