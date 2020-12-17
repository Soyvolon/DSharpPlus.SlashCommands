using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Microsoft.AspNetCore.Http;

namespace DSharpPlus.SlashCommands.Entities
{
    public class InteractionContext
    {
        private readonly DiscordSlashClient _client;
        private readonly HttpRequest request;

        public Interaction Interaction { get; internal set; }

        public InteractionContext(DiscordSlashClient c, Interaction i, HttpRequest r)
        {
            _client = c;
            Interaction = i;
            request = r;
        }

        /// <summary>
        /// Reply to the interaction
        /// </summary>
        /// <param name="message">Text content to send back</param>
        /// <param name="embed">Embed to send back</param>
        /// <param name="tts">Is this message a text to speech message?</param>
        /// <returns>Reply task</returns>
        public async Task<InteractionResponse> ReplyAsync(string message = "", DiscordEmbed? embed = null, bool? tts = null)
        {
            throw new NotImplementedException();
        }
    }
}
