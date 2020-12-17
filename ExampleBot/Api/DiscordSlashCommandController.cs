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
            string raw;
            // Request validation
            try
            {
                var signature = Request.Headers["X-Signature-Ed25519"].ToString();
                var timestamp = Request.Headers["X-Signature-Timestamp"].ToString();

                using var rsa = new RSACryptoServiceProvider();

                var byteKey = Utils.HexToBytes(Startup.PublicKey);
                var byteSig = Utils.HexToBytes(signature);

                var reader = new StreamReader(Request.Body);
                if (reader.BaseStream.CanSeek)
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                raw = await reader.ReadToEndAsync();

                var sha = SHA256.Create();

                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(timestamp + raw));

                var bodyHash = await sha.ComputeHashAsync(ms);

                rsa.ImportRSAPublicKey(byteKey, out _);

                bool verified = rsa.VerifyHash(bodyHash, CryptoConfig.MapNameToOID("SHA256"), byteSig);

                if (!verified)
                    return Unauthorized("Invalid Request Signature");
            }
            catch
            {
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
