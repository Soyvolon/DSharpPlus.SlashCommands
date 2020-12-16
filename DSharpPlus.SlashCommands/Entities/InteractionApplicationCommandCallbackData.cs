using System.Collections.Generic;

using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace DSharpPlus.SlashCommands.Entities
{
    public class InteractionApplicationCommandCallbackData
    {
        [JsonProperty("tts")]
        public bool? TextToSpeech { get; internal set; }
        [JsonProperty("content")]
        public string Content { get; internal set; }
        [JsonProperty("embeds")]
        public DiscordEmbed[]? Embeds { get; internal set; }
        [JsonProperty("allowed_mentions")]
        public IEnumerable<IMention>? AllowedMentions { get; internal set; }
    }
}
