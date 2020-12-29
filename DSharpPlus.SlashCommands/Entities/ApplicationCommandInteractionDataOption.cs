
using DSharpPlus.SlashCommands.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.SlashCommands.Entities
{
    public class ApplicationCommandInteractionDataOption
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("value")]
        public object? Value { get; internal set; }
        [JsonProperty("options")]
        public ApplicationCommandInteractionDataOption[]? Options { get; internal set; }
    }
}
