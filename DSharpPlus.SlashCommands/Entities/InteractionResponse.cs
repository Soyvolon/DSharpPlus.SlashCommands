using DSharpPlus.SlashCommands.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.SlashCommands.Entities
{
    public class InteractionResponse
    {
        [JsonProperty("type")]
        public InteractionResponseType Type { get; internal set; }
        [JsonProperty("data")]
        public InteractionApplicationCommandCallbackData? Data { get; internal set; }
    }
}
