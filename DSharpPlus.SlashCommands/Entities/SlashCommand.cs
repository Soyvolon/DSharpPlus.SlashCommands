using System;
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.SlashCommands.Entities
{
    public class SlashCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? CommandId { get; set; }
        public ulong? ApplicationId { get; set; }
        public int Version { get; set; }
        public SlashSubcommand? Command { get; init; }

        public Dictionary<string, SlashSubcommandGroup>? Subcommands { get; init; }

        public SlashCommand(string name, int version, SlashSubcommand command, ulong? gid)
        {
            Name = name;
            Version = version;
            Description = command.Description;
            GuildId = gid;
            Command = command;
            Subcommands = null;
        }

        public SlashCommand(string name, int version, SlashSubcommandGroup[] subcommands, ulong? gid, string desc = "n/a")
        {
            Name = name;
            Version = version;
            Description = desc;
            Subcommands = subcommands.ToDictionary(x => x.Name);
            GuildId = gid;
            Command = null;
        }

        /// <summary>
        /// Attempts to execute a command from a command with no subcommands.
        /// </summary>
        /// <param name="args">Command arguments</param>
        /// <returns>True if the command was attempted, false if there was no command to attempt.</returns>
        public bool ExecuteCommand(params object[] args)
        {
            if (Command is not null)
            {
                Command.ExecuteCommand(args);
                return true;
            }

            return false;
        }

        public SlashCommandConfiguration GetConfiguration()
        {
            return new SlashCommandConfiguration()
            {
                CommandId = CommandId ?? throw new Exception("Failed to get a valid command ID"),
                GuildId = GuildId,
                Name = Name,
                Version = Version
            };
        }
    }
}
