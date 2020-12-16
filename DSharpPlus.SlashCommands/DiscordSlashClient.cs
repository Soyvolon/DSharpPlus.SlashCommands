using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace DSharpPlus.SlashCommands
{
    public class DiscordSlashClient
    {
        private readonly IServiceProvider services;
        private readonly DiscordSlashConfiguration config;

        public DiscordSlashClient(DiscordSlashConfiguration config)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<SlashCommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddLogging(o => o.AddConsole());

            this.services = services.BuildServiceProvider();
            this.config = config;
        }

        /// <summary>
        /// Add an assembly to register commands from.
        /// </summary>
        /// <param name="assembly">Assembly to register</param>
        public void WithAssembly(Assembly assembly)
        {
            var slash = services.GetRequiredService<SlashCommandHandlingService>();
            slash.WithCommandAssembly(assembly);
        }

        /// <summary>
        /// Starts the slash command client.
        /// </summary>
        /// <returns>Start operation</returns>
        public async Task StartAsync()
        {
            var slash = services.GetRequiredService<SlashCommandHandlingService>();
            await slash.StartAsync(config.Token, config.ClientId);
        }

        /// <summary>
        /// Handle an incoming webhook request.
        /// </summary>
        /// <param name="request">HttpRequest for the interaction POST</param>
        /// <returns>Handle Webhook operation</returns>
        public async Task HandleWebhookPost(HttpRequest request)
        {

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
    }
}
