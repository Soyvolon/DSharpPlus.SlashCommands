using System;

namespace DSharpPlus.SlashCommands.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SlashSubcommandGroupAttribute : Attribute
    {
        public string Name { get; set; }
        public SlashSubcommandGroupAttribute(string name)
        {
            Name = name;
        }
    }
}
