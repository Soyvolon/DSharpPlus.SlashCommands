
using Newtonsoft.Json;

namespace DSharpPlus.SlashCommands.Entities
{
    public class ApplicationCommand
    {
        [JsonProperty("id")]
        public ulong Id { get; internal set; }
        [JsonProperty("application_id")]
        public ulong ApplicationId { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Options/Subcommands - Max of 10.
        /// </summary>
        [JsonProperty("options")]
        public ApplicationCommandOption[]? Options { get; internal set; }

        internal ApplicationCommand()
        {
            Name = "";
            Description = "";
        }
    }
}
