
using Newtonsoft.Json;

namespace DSharpPlus.SlashCommands.Entities
{
    public class ApplicationCommandOptionChoice
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Must be string or int
        /// </summary>
        [JsonProperty("value")]
        public object Value { get; internal set; }

        internal ApplicationCommandOptionChoice() : this("", 0) { }

        public ApplicationCommandOptionChoice(string n, int v)
        {
            Name = n;
            Value = v;
        }

        public ApplicationCommandOptionChoice(string n, string v)
        {
            Name = n;
            Value = v;
        }
    }
}
