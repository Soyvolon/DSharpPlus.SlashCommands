using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Sodium;

namespace ExampleBot.Api
{
    [Route("api/discordslash")]
    [ApiController]
    public class DiscordSlashCommandController : ControllerBase
    {
        private readonly ILogger _logger;

        public DiscordSlashCommandController(ILogger<DiscordSlashCommandController> logger)
        {
            _logger = logger;
        }

        private struct DiscordChallenge
        {
            [JsonProperty("type")]
            public int type { get; set; }
        }

        [HttpPost("")]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DiscordEndpointHandler()
        {
            string raw;
            // Request validation
            try
            {
                var signature = Request.Headers["X-Signature-Ed25519"].ToString();
                var timestamp = Request.Headers["X-Signature-Timestamp"].ToString();

                var byteSig = Utils.HexToBytes(signature);
                var byteKey = Utils.HexToBytes(Startup.PublicKey);

                using var reader = new StreamReader(Request.Body);
                if (reader.BaseStream.CanSeek)
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                raw = await reader.ReadToEndAsync();

                string body = timestamp + raw;
                var byteBody = Encoding.Default.GetBytes(body);

                bool validated = PublicKeyAuth.VerifyDetached(byteSig, byteBody, byteKey);

                if(!validated)
                {
                    _logger.LogWarning("Failed to validate POST request for Discord API.");
                    return Unauthorized("Invalid Request Signature");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Decryption failed.");
                _logger.LogWarning("Failed to validate POST request for Discord API.");
                return Unauthorized("Invalid Request Signature");
            }

            // Response parsing
            JObject json;
            try
            {
                
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
