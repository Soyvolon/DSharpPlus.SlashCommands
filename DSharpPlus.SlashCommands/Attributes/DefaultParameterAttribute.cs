using System;

namespace DSharpPlus.SlashCommands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class DefaultParameterAttribute : Attribute
    {
    }
}
