using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.SlashCommands.Entities
{
    public class SlashSubcommandGroup
    {
        public string Name { get; init; }
        public Dictionary<string, SlashSubcommand> Commands { get; init; }
        public SlashSubcommandGroup(string name, SlashSubcommand[] commands)
        {
            Name = name;
            Commands = commands.ToDictionary(x => x.Name);
        }

        public SlashSubcommandGroup(string name)
        {
            Name = name;
            Commands = new();
        }
    }
}
