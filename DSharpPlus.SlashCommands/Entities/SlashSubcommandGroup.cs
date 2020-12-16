using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.SlashCommands.Entities
{
    public class SlashSubcommandGroup
    {
        public string Name { get; init; }
        public string Description { get; set; }
        public bool? Required { get; set; }
        public bool? Default { get; set; }
        public Dictionary<string, SlashSubcommand> Commands { get; init; }
        public SlashSubcommandGroup(string name, string description, SlashSubcommand[] commands)
        {
            Name = name;
            Description = description;
            Commands = commands.ToDictionary(x => x.Name);
        }

        public SlashSubcommandGroup(string name, string description)
        {
            Name = name;
            Description = description;
            Commands = new();
        }
    }
}
