using System;

namespace DSharpPlus.SlashCommands.Entities
{
    public class SlashCommandBase
    {
        protected readonly IServiceProvider _services;

        public SlashCommandBase(IServiceProvider services)
        {
            _services = services;
        }
    }
}
