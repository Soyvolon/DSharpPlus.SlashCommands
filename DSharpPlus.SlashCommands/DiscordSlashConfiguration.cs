using System;

using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Enums;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.SlashCommands
{
    public class DiscordSlashConfiguration
    {
        /// <summary>
        /// Token used for the Discrod Bot user that your application has.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Base client used for parsing DSharpPlus arguments. Can be a Rest or Regular client. This or a ShardedClient is required.
        /// </summary>
        public BaseDiscordClient? Client { get; set; }
        /// <summary>
        /// Base sharded client used for parsing DSharpPLus arguments. This or a Rest or Regular client is required.
        /// </summary>
        public DiscordShardedClient? ShardedClient { get; set; }
        /// <summary>
        /// The Default Response type that is sent to Discord upon receiving a request.
        /// </summary>
        public InteractionResponseType DefaultResponseType { get; set; } = InteractionResponseType.DeferredChannelMessageWithSource;
        /// <summary>
        /// The default data to be used when the DefaultResponseType is ChannelMessage or ChannelMessageWithSource.
        /// </summary>
        public InteractionApplicationCommandCallbackDataBuilder? DefaultResponseData { get; set; } = null;
        /// <summary>
        /// Supply a logger to override the DSharpPlus logger.
        /// </summary>
        public ILogger? Logger { get; set; } = null;
        /// <summary>
        /// Services for dependency injection for slash commands.
        /// </summary>
        public IServiceProvider? Services { get; set; } = null;
    }
}
