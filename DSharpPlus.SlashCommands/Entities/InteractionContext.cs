using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.Entities
{
    public class InteractionContext
    {
        private readonly DiscordSlashClient _client;

        public Interaction Interaction { get; internal set; }

        public InteractionContext(DiscordSlashClient c, Interaction i)
        {
            _client = c;
            Interaction = i;
        }
    }
}
