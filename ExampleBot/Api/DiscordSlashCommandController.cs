using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExampleBot.Api
{
    [Route("api/discordslash")]
    [ApiController]
    public class DiscordSlashCommandController : ControllerBase
    {
        private struct DiscordChallenge
        {
            [JsonProperty("type")]
            public int type { get; set; }
        }

        [HttpPost("")]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DiscordEndpointHandler()
        {
            JObject json;
            try
            {
                var reader = new StreamReader(Request.Body);
                if (reader.BaseStream.CanSeek)
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                var raw = await reader.ReadToEndAsync();
                json = JObject.Parse(raw);
            }
            catch
            {
                return BadRequest();
            }

            if (json.ContainsKey("type") && (int)json["type"] == 1)
                return Ok(json.ToString(Formatting.None));
            else return Ok(); // trigger handle webhook post in DiscordSlashClient.
        }
    }
}
