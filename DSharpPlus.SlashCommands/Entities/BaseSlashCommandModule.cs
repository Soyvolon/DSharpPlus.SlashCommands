using System;

namespace DSharpPlus.SlashCommands.Entities
{
    public class BaseSlashCommandModule
    {
        protected readonly IServiceProvider _services;

        public BaseSlashCommandModule(IServiceProvider services)
        {
            _services = services;
        }
    }
}
