using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Enums;

using Microsoft.AspNetCore.Http;

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
        #region Followup
        /// <summary>
        /// Reply to the interaction by sending a followup message.
        /// </summary>
        /// <param name="message">Text content to send back</param>
        /// <param name="embeds">Embeds to send back</param>
        /// <param name="tts">Is this message a text to speech message?</param>
        /// <param name="allowedMentions">The allowed mentions of the message</param>
        /// <returns>The response object form discord</returns>
        public async Task<DiscordMessage> ReplyAsync(string message = "", DiscordEmbed[]? embeds = null, bool? tts = null, IMention[]? allowedMentions = null, bool showSource = false)
        {
            if (embeds is not null && embeds.Length > 10)
                throw new Exception("Too many embeds");

            return await ReplyAsync(new InteractionResponse()
            {
                Type = showSource ? InteractionResponseType.ChannelMessageWithSource : InteractionResponseType.ChannelMessage,
                Data = new InteractionApplicationCommandCallbackData()
                {
                    Content = message,
                    Embeds = embeds,
                    TextToSpeech = tts,
                    AllowedMentions = allowedMentions
                }
            });
        }
        
        /// <summary>
        /// Reply to the interaction by sending a followup message.
        /// </summary>
        /// <param name="response">An InteractionResponse object to send to the user.</param>
        /// <returns>The response object form discord</returns>
        public async Task<DiscordMessage> ReplyAsync(InteractionResponse response)
        {
            var msg = await _client.FollowupWithAsync(response, Interaction.Token);

            if (msg is null)
                throw new Exception("Failed to reterive message object from Discord.");

            return msg;
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
        /// <param name="allowedMentions">Allowed mentions list</param>
        /// <returns>The edited response</returns>
        public async Task<DiscordMessage> EditResponseAsync(string message = "", ulong toEdit = 0, DiscordEmbed[]? embeds = null, bool? tts = null, IMention[]? allowedMentions = null)
        {
            if (embeds is not null && embeds.Length > 10)
                throw new Exception("Too many embeds");

            return await EditResponseAsync(new InteractionResponse()
            { 
                Type = InteractionResponseType.ChannelMessage,
                Data = new InteractionApplicationCommandCallbackData()
                { 
                    Content = message,
                    Embeds = embeds,
                    TextToSpeech = tts,
                    AllowedMentions = allowedMentions
                }
            },
            toEdit);
        }

        /// <summary>
        /// Edits an already sent message
        /// </summary>
        /// <param name="response">InteractionResponse to send to the user</param>
        /// <param name="toEdit">Message to edit by ID of the order it was sent. Defaults to the inital response</param>
        /// <returns>The edited response</returns>
        public async Task<DiscordMessage> EditResponseAsync(InteractionResponse response, ulong toEdit = 0)
        {
            DiscordMessage? msg;
            if(toEdit == 0)
            {
                msg = await _client.UpdateAsync(response, Interaction.Token);
            }
            else
            {
                msg = await _client.EditAsync(response, Interaction.Token, toEdit);
            }

            if (msg is null)
                throw new Exception("Failed to edit the message");

            return msg;
        }
        #endregion
        #region Delete
        /// <summary>
        /// Deletes the inital response for this interaction.
        /// </summary>
        /// <returns>The deleted interaction</returns>
        public async Task<DiscordMessage> DeleteInitalAsync()
        {
            var msg = await _client.DeleteAsync(Interaction.Token);

            if (msg is null)
                throw new Exception("Failed to delete origial message");

            return msg;
        }
        #endregion
    }
}
