using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.Entities;
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
        private readonly SlashCommandHandlingService _slash;
        private readonly DiscordSlashConfiguration _config;
        private readonly HttpClient _http;
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        private const string _contentType = "application/json";

        public DiscordSlashClient(DiscordSlashConfiguration config)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<SlashCommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddLogging(o => o.AddConsole());

            this._services = services.BuildServiceProvider();
            this._logger = this._services.GetRequiredService<ILogger<DiscordSlashClient>>();
            this._slash = this._services.GetRequiredService<SlashCommandHandlingService>();
            this._config = config;
            this._http = this._services.GetRequiredService<HttpClient>();
            this._http.DefaultRequestHeaders.Authorization = new("Bot", this._config.Token);
        }

        /// <summary>
        /// Add an assembly to register commands from.
        /// </summary>
        /// <param name="assembly">Assembly to register</param>
        public void RegisterCommands(Assembly assembly)
        {
            _slash.WithCommandAssembly(assembly);
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
            await _slash.StartAsync(_config.Token, _config.ClientId);
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
                // ... and tell the handler to run the command ...

                var jobj = JObject.Parse(requestBody);
                DiscordUser? user = jobj["member"]?["user"]?.ToObject<DiscordUser>();

                i.User = user;

                await _slash.HandleInteraction(i, this);
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
        /// <returns>The DiscordMessage that was created.</returns>
        internal async Task<DiscordMessage?> FollowupWithAsync(InteractionResponse followup, string token)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = GetPostFollowupUri(token);
            string json = followup.BuildWebhookBody(_jsonSettings);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new(_contentType);

            var res = await _http.SendAsync(request);

            if (res.IsSuccessStatusCode)
            {
                try
                {
                    var resJson = await res.Content.ReadAsStringAsync();
                    var msg = JsonConvert.DeserializeObject<DiscordMessage>(resJson);
                    return msg;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Followup With Async Failed");
                    return null;
                }
            }
            else return null;
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
            return new Uri($"{api}/webhooks/{_config.ClientId}/{token}&wait={_config.WaitForConfirmation}");
        }

        protected Uri GetEditFollowupUri(string token, ulong messageId)
        {
            return new Uri($"{api}/webhooks/{_config.ClientId}/{token}/messages/{messageId}");
        }
    }
}
