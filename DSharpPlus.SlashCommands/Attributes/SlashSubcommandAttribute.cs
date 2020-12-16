using System;

namespace DSharpPlus.SlashCommands.Attributes
{
    /// <summary>
    /// Defines a method as the default command for a command grouping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SlashSubcommandAttribute : Attribute
    {
        public string Name { get; init; }

        public SlashSubcommandAttribute(string name)
        {
            Name = name;
        }
    }
}
