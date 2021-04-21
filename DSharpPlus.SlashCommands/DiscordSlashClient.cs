using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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

        private ulong ApplicationId
        {
            get
            {
                if (this._discord is not null)
                    return this._discord.CurrentApplication.Id;
                else if (this._sharded is not null)
                    return this._sharded.CurrentApplication.Id;
                else return 0;
            }
        }

        private readonly IServiceProvider _services;
        private readonly SlashCommandHandlingService _slash;
        private readonly DiscordSlashConfiguration _config;
        private readonly BaseDiscordClient? _discord;
        private readonly DiscordShardedClient? _sharded;
        private readonly HttpClient _http;
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        private const string _contentType = "application/json";

        public DiscordSlashClient(DiscordSlashConfiguration config, IServiceCollection? collection = null)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<SlashCommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddLogging(o => o.AddConsole());

            if(collection is not null)
            {
                foreach (var c in collection)
                    services.Add(c);
            }

            this._services = services.BuildServiceProvider();
            this._logger = this._services.GetRequiredService<ILogger<DiscordSlashClient>>();
            this._slash = this._services.GetRequiredService<SlashCommandHandlingService>();
            this._config = config;
            this._http = this._services.GetRequiredService<HttpClient>();
            this._http.DefaultRequestHeaders.Authorization = new("Bot", this._config.Token);
            this._discord = this._config.Client;
            this._sharded = this._config.ShardedClient;

            if (this._discord is null && this._sharded is null)
                throw new Exception("A Discord Client or Sharded Client is required.");
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
            if ((_config.DefaultResponseType != InteractionResponseType.ChannelMessageWithSource)
                && _config.DefaultResponseData is null)
                throw new Exception("DeafultResponseData must not be null if not using ResponseType of Acknowledge or ACKWithSource.");
                

            // Initialize the command handling service (and therefor updating command on discord).
            await _slash.StartAsync(_config.Token, ApplicationId);
        }

        public async Task<bool> HandleGatewayEvent(DiscordClient client, InteractionCreateEventArgs args)
        {
            await _slash.HandleInteraction(client, args.Interaction, this);

            var data = GetDeafultResponse().Build();

            var msg = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = GetGatewayFollowupUri(args.Interaction.Id.ToString(), args.Interaction.Token),
                Content = new StringContent(JsonConvert.SerializeObject(data, _jsonSettings))
            };

            msg.Content.Headers.ContentType = new(_contentType);

            var res = await _http.SendAsync(msg);

            return res.IsSuccessStatusCode;
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
                var i = JsonConvert.DeserializeObject<DiscordInteraction>(requestBody);
                // ... and tell the handler to run the command ...

                //var jobj = JObject.Parse(requestBody);
                //DiscordUser? user = jobj["member"]?["user"]?.ToObject<DiscordUser>();
                //// ... because we cant serialize direct to a DiscordMember, we are working around this
                //// and using a DiscordUser instead. I would have to set the Lib as upstream to this before I
                //// would be able to change this.
                //i.User = user;

                var client = GetBaseClientForRequest(i.GuildId);

                await _slash.HandleInteraction(client, i, this);
            }
            catch (Exception ex)
            { // ... if it errors, log and return null.
                _logger.LogError(ex, "Webhook Handler failed.");
                return null;
            }

            return GetDeafultResponse().Build();
        }

        private BaseDiscordClient GetBaseClientForRequest(ulong? guildId = null)
        {
            BaseDiscordClient? client = null;
            if (_discord is not null)
                client = _discord;

            if (client is null)
            {
                if (guildId is null)
                {
                    if(_sharded is not null && _sharded.ShardClients.Count > 0)
                    {
                        client = _sharded.ShardClients[0];
                    }
                }
                else
                {
                    if (_sharded is not null)
                    {
                        foreach (var shard in _sharded.ShardClients)
                            if (shard.Value.Guilds.ContainsKey(guildId.Value))
                                client = shard.Value;
                    }
                }
            }

            if (client is null)
                throw new Exception("Failed to get a proper cleint for this request.");

            return client;
        }

        private InteractionResponseBuilder GetDeafultResponse()
        {
            // createa  new response object ....
            var response = new InteractionResponseBuilder()
                .WithType(_config.DefaultResponseType);
            // ... add the optional configs ...
            if (_config.DefaultResponseType == InteractionResponseType.ChannelMessageWithSource)
            {
                response.Data = _config.DefaultResponseData;
            }
            // ... and return the builder object.
            return response;
        }

        /// <summary>
        /// Updates the original interaction response.
        /// </summary>
        /// <param name="edit">New version of the response</param>
        /// <returns>Update task</returns>
        internal async Task<DiscordMessage?> UpdateAsync(InteractionResponse edit, string token)
        {
            if(_config.DefaultResponseType == InteractionResponseType.ChannelMessageWithSource)
                throw new Exception("Can't edit default response when using Acknowledge or ACKWithSource.");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = GetEditOrDeleteInitialUri(token),
                Content = new StringContent(edit.BuildWebhookEditBody(_jsonSettings)),
            };
            request.Content.Headers.ContentType = new(_contentType);

            var res = await _http.SendAsync(request);

            if (res.IsSuccessStatusCode)
            {
                return await GetResponseBody(res);
            }
            else return null;
        }

        /// <summary>
        /// Deletes the original response
        /// </summary>
        /// <param name="token">Token for the default interaction to be delete.</param>
        /// <returns>Delete task</returns>
        internal async Task<DiscordMessage?> DeleteAsync(string token)
        {
            if(_config.DefaultResponseType == InteractionResponseType.ChannelMessageWithSource)
                throw new Exception("Can't delete default response when using Acknowledge or ACKWithSource.");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = GetEditOrDeleteInitialUri(token),
            };

            var res = await _http.SendAsync(request);

            if (res.IsSuccessStatusCode)
            {
                return await GetResponseBody(res);
            }
            else return null;
        }

        /// <summary>
        /// Follow up the interaction response with a new response.
        /// </summary>
        /// <param name="followup">New response to send.</param>
        /// <param name="token">Original response token.</param>
        /// <returns>The DiscordMessage that was created.</returns>
        internal async Task<DiscordMessage?> FollowupWithAsync(InteractionResponse followup, string token)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = GetPostFollowupUri(token),
                Content = new StringContent(followup.BuildWebhookBody(_jsonSettings))
            };
            request.Content.Headers.ContentType = new(_contentType);

            var res = await _http.SendAsync(request);

            if (res.IsSuccessStatusCode)
            {
                return await GetResponseBody(res);
            }
            else return null;
        }

        /// <summary>
        /// Edits a followup message from a response.
        /// </summary>
        /// <param name="message">New message to replace the old one with.</param>
        /// <param name="token">Original response token.</param>
        /// <param name="id">Id of the followup message that you want to edit.</param>
        /// <returns>Edit task</returns>
        internal async Task<DiscordMessage?> EditAsync(InteractionResponse edit, string token, ulong id)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = GetEditFollowupUri(token, id),
                Content = new StringContent(edit.BuildWebhookEditBody(_jsonSettings)),
            };
            request.Content.Headers.ContentType = new(_contentType);

            var res = await _http.SendAsync(request);

            if (res.IsSuccessStatusCode)
            {
                return await GetResponseBody(res);
            }
            else return null;
        }

        private async Task<DiscordMessage?> GetResponseBody(HttpResponseMessage res)
        {
            try
            {
                var resJson = await res.Content.ReadAsStringAsync();
                var msg = JsonConvert.DeserializeObject<DiscordMessage>(resJson);
                return msg;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Original Async Failed");
                return null;
            }
        }

        protected Uri GetEditOrDeleteInitialUri(string token)
        {
            return new Uri($"{api}/webhooks/{ApplicationId}/{token}/messages/@original");
        }

        protected Uri GetPostFollowupUri(string token)
        {
            return new Uri($"{api}/webhooks/{ApplicationId}/{token}?wait={_config.WaitForConfirmation}");
        }

        protected Uri GetEditFollowupUri(string token, ulong messageId)
        {
            return new Uri($"{api}/webhooks/{ApplicationId}/{token}/messages/{messageId}");
        }

        protected Uri GetGatewayFollowupUri(string interactId, string token)
        {
            return new Uri($"{api}/interactions/{interactId}/{token}/callback");
        }
    }
}
