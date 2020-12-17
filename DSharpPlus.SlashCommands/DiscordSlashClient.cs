using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.SlashCommands
{
    public class DiscordSlashClient
    {
        private const string api = "https://discord.com/api/v8";
        private readonly IServiceProvider _services;
        private readonly DiscordSlashConfiguration _config;
        private readonly ILogger _logger;

        public DiscordSlashClient(DiscordSlashConfiguration config)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<SlashCommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddLogging(o => o.AddConsole());

            this._services = services.BuildServiceProvider();
            this._logger = this._services.GetRequiredService<ILogger<DiscordSlashClient>>();
            this._config = config;
        }

        /// <summary>
        /// Add an assembly to register commands from.
        /// </summary>
        /// <param name="assembly">Assembly to register</param>
        public void WithAssembly(Assembly assembly)
        {
            var slash = _services.GetRequiredService<SlashCommandHandlingService>();
            slash.WithCommandAssembly(assembly);
        }

        /// <summary>
        /// Starts the slash command client.
        /// </summary>
        /// <returns>Start operation</returns>
        public async Task StartAsync()
        {
            // Set this restriction to ensure proper response for async command handling.
            if (_config.DefaultResponse != Enums.InteractionResponseType.Acknowledge
                && _config.DefaultResponse != Enums.InteractionResponseType.ACKWithSource)
                throw new Exception("Default response must be Acknowledge or ACKWithSource.");

            // Initalize the command handling service (and therefor updating command on discord).
            var slash = _services.GetRequiredService<SlashCommandHandlingService>();
            await slash.StartAsync(_config.Token, _config.ClientId);
        }

        /// <summary>
        /// Handle an incoming webhook request and return the default data to send back to Discord.
        /// </summary>
        /// <param name="request">HttpRequest for the interaction POST</param>
        /// <returns>Handle Webhook operation</returns>
        public async Task<InteractionResponse?> HandleWebhookPost(string requestBody)
        {
            try
            {// Attempt to get the Interact object from the JSON ...
                var i = JsonConvert.DeserializeObject<Interaction>(requestBody);
                // ... get the command handling service if it parses ...
                var slash = _services.GetRequiredService<SlashCommandHandlingService>();
                // ... and tell the handler to run the command ...
                await slash.HandleInteraction(i);
            }
            catch (Exception ex)
            { // ... if it errors, log and return null.
                _logger.LogError(ex, "Webhook Handler failed.");
                return null;
            }
            // ... return the default interaction type.
            return new InteractionResponseBuilder()
                .WithType(_config.DefaultResponse)
                .Build();
        }

        /// <summary>
        /// Updates the origial interaction response.
        /// </summary>
        /// <param name="iteraction">New version of the response</param>
        /// <returns>Update task</returns>
        internal async Task UpdateAsync(InteractionResponse iteraction)
        {

        }

        /// <summary>
        /// Deletes the origial response
        /// </summary>
        /// <param name="interaction">Interacton to delete.</param>
        /// <returns>Delete task</returns>
        internal async Task DeleteAsync(InteractionResponse interaction)
        {

        }

        /// <summary>
        /// Follow up the interaction response with a new response.
        /// </summary>
        /// <param name="followup">New response to send.</param>
        /// <param name="token">Origianl response token.</param>
        /// <returns>Follwup task</returns>
        internal async Task FollowupWithAsync(InteractionResponse followup, string token)
        {

        }

        /// <summary>
        /// Edits a followup message from a response.
        /// </summary>
        /// <param name="message">New message to replace the old one with.</param>
        /// <param name="token">Origial response token.</param>
        /// <param name="id">Id of the followup message that you want to edit.</param>
        /// <returns>Edit task</returns>
        internal async Task EditAsync(InteractionResponse message, string token, ulong id)
        {

        }

        protected Uri GetEditOrDeleteInitalUri(string token)
        {
            return new Uri($"{api}/webhooks/{_config.ClientId}/{token}/@original");
        }

        protected Uri GetPostFollowupUri(string token)
        {
            return new Uri($"{api}/webhooks/{_config.ClientId}/{token}");
        }

        protected Uri GetEditFollowupUri(string token, ulong messageId)
        {
            return new Uri($"{api}/webhooks/{_config.ClientId}/{token}/messages/{messageId}");
        }
    }
}
