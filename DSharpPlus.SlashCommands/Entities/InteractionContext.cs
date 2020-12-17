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
        #region Followup
        /// <summary>
        /// Reply to the interaction by sending a followup message.
        /// </summary>
        /// <param name="message">Text content to send back</param>
        /// <param name="embeds">Embeds to send back</param>
        /// <param name="tts">Is this message a text to speech message?</param>
        /// <param name="AllowedMentions">The allowed mentions of the message</param>
        /// <returns>The response object form discord</returns>
        public async Task<InteractionResponse> ReplyAsync(string message = "", DiscordEmbed[]? embeds = null, bool? tts = null, IMention[]? AllowedMentions = null)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Reply to the interaction by sending a followup message.
        /// </summary>
        /// <param name="response">An InteractionResponse object to send to the user.</param>
        /// <returns>The response object form discord</returns>
        public async Task<InteractionResponse> ReplyAsync(InteractionResponse response)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Edit
        // Edit for both inital response and other responses.
        /// <summary>
        /// Edits an already sent message.
        /// </summary>
        /// <param name="message">Text content to send</param>
        /// <param name="toEdit">Message to edit by ID of the order it was sent. Defaults to the inital response</param>
        /// <param name="embeds">Embeds to send.</param>
        /// <param name="tts">Is this Text To Speech?</param>
        /// <param name="AllowedMentions">Allowed mentions list</param>
        /// <returns>The edited response</returns>
        public async Task<InteractionResponse> EditResponseAsync(string message = "", int toEdit = 0, DiscordEmbed[]? embeds = null, bool? tts = null, IMention[]? AllowedMentions = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Edits an already sent message
        /// </summary>
        /// <param name="response">InteractionResponse to send to the user</param>
        /// <param name="toEdit">Message to edit by ID of the order it was sent. Defaults to the inital response</param>
        /// <returns>The edited response</returns>
        public async Task<InteractionResponse> EditResponseAsync(InteractionResponse response, int toEdit = 0)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Delete
        /// <summary>
        /// Deletes the inital response for this interaction.
        /// </summary>
        /// <returns>The deleted interaction</returns>
        public async Task<InteractionResponse> DeleteInitalAsync()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
