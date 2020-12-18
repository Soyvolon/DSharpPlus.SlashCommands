using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Enums;

namespace DSharpPlus.SlashCommands
{
    public class DiscordSlashConfiguration
    {
        /// <summary>
        /// Token used for the Discrod Bot user that your application has.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// The Client ID of your application
        /// </summary>
        public ulong ClientId { get; set; }
        /// <summary>
        /// The Default Response type that is sent to Discord upon receving a request.
        /// </summary>
        public InteractionResponseType DefaultResponseType { get; set; } = InteractionResponseType.ACKWithSource;
        /// <summary>
        /// The default data to be used when the DefaultResponseType is ChannelMessage or ChannelMessageWithSource.
        /// </summary>
        public InteractionApplicationCommandCallbackDataBuilder? DefaultResponseData { get; set; } = null;
        /// <summary>
        /// If Discord is to wait for confirmation that a message has been saved before sending a reply back to the SlashClient.
        /// </summary>
        public bool WaitForConfirmation { get; set; } = true;
    }
}
