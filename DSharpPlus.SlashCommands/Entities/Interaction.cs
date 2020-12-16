
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.SlashCommands.Entities
{
    public class Interaction
    {
        [JsonProperty("id")]
        public ulong Id { get; internal set; }
        [JsonProperty("type")]
        public InteractionType Type { get; internal set; }
        [JsonProperty("data")]
        public ApplicationCommandInteractionData? Data { get; internal set; }
        [JsonProperty("guild_id")]
        public ulong GuildId { get; internal set; }
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; internal set; }
        [JsonProperty("member")]
        public DiscordMember Member { get; internal set; }
        [JsonProperty("token")]
        public string Token { get; internal set; }
        [JsonProperty("version")]
        public int Version { get; internal set; }
    }
}
