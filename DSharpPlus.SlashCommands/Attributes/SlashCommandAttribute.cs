using System;

namespace DSharpPlus.SlashCommands.Attributes
{
    /// <summary>
    /// Used to designate a class as a slash command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    internal class SlashCommandAttribute : Attribute
    {
        public string Name { get; init; }
        public int Version { get; init; }
        public ulong? GuildId { get; init; }

        public SlashCommandAttribute(string name, int version = 1, ulong guildId = 0)
        {
            Name = name;
            Version = version;
            GuildId = guildId == 0 ? null : guildId;
        }
    }
}