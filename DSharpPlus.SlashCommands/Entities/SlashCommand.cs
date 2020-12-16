using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.SlashCommands.Entities
{
    public class SlashCommand
    {
        public string Name { get; set; }

        public SlashSubcommand? Command { get; init; }

        public Dictionary<string, SlashSubcommandGroup>? Subcommands { get; init; }

        public SlashCommand(string name, SlashSubcommand command)
        {
            Name = name;
            Command = command;
            Subcommands = null;
        }

        public SlashCommand(string name, SlashSubcommandGroup[] subcommands)
        {
            Name = name;
            Subcommands = subcommands.ToDictionary(x => x.Name);
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
    }
}
