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
        public InteractionResponseType DefaultResponse { get; set; } = InteractionResponseType.ACKWithSource;
    }
}
