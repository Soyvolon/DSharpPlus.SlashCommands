using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace DSharpPlus.SlashCommands.Entities
{
    public class SlashCommandConfiguration
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("guild_id")]
        public ulong? GuildId { get; set; }
        [JsonProperty("command_id")]
        public ulong CommandId { get; set; }
    }
}
