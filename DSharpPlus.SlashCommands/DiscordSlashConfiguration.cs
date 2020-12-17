using DSharpPlus.SlashCommands.Enums;

namespace DSharpPlus.SlashCommands
{
    public class DiscordSlashConfiguration
    {
        public string Token { get; set; }
        public ulong ClientId { get; set; }
        public InteractionResponseType DefaultResponse { get; set; } = InteractionResponseType.ACKWithSource;
    }
}
