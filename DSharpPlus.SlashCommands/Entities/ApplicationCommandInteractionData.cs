
using Newtonsoft.Json;

namespace DSharpPlus.SlashCommands.Entities
{
    public class ApplicationCommandInteractionData
    {
        [JsonProperty("id")]
        public ulong Id { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("options")]
        public ApplicationCommandInteractionDataOption[]? Options { get; internal set; }
    }
}
