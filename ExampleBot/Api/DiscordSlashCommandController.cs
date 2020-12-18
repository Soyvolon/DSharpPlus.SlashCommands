using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Enums;

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

        [HttpPost("")]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DiscordEndpointHandler()
        {
            string raw;
            // Request validation
            try
            {
                // Get the verification headers from the request ...
                var signature = Request.Headers["X-Signature-Ed25519"].ToString();
                var timestamp = Request.Headers["X-Signature-Timestamp"].ToString();
                // ... convert the signature and public key to byte[] to use in verification ...
                var byteSig = Utils.HexStringToByteArray(signature);
                var byteKey = Utils.HexStringToByteArray(Startup.PublicKey);
                // ... read the body from the request ...
                using var reader = new StreamReader(Request.Body);
                if (reader.BaseStream.CanSeek)
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                raw = await reader.ReadToEndAsync();
                // ... add the timestamp and convert it to a byte[] ...
                string body = timestamp + raw;
                var byteBody = Encoding.Default.GetBytes(body);
                // ... run through a verification with all the byte[]s ...
                bool validated = PublicKeyAuth.VerifyDetached(byteSig, byteBody, byteKey);
                // ... if it is not validated ...
                if(!validated)
                {   // ... log a warning and return a 401 Unauthorized.
                    _logger.LogWarning("Failed to validate POST request for Discord API.");
                    return Unauthorized("Invalid Request Signature");
                }
                else
                { // ... otherwise continue onwards.
                    _logger.LogInformation("Received POST from Discord");
                }
            }
            catch (Exception ex)
            { // ... if an error occurred, log the error and return at 401 Unauthorized.
                _logger.LogInformation(ex, "Decryption failed.");
                _logger.LogWarning("Failed to validate POST request for Discord API.");
                return Unauthorized("Invalid Request Signature");
            }

            // Response parsing
            JObject json;
            try
            { // ... attempt to create a json object from the body ...
                json = JObject.Parse(raw);
            }
            catch
            { // ... if that fails, return a 400 Bad Request.
                return BadRequest();
            }
            // ... check to see if this is a ping to the webhook ...
            if (json.ContainsKey("type") && (int)json["type"] == (int)InteractionType.Ping)
            {
                return Ok(
                    JsonConvert.SerializeObject(
                        new InteractionResponseBuilder()
                            .WithType(InteractionResponseType.Pong)
                            .Build()
                        )
                    ); // ... and return the pong if it is.
            }
            else
            {// ... then pass the raw request body to the client ...
                var response = await Program.Slash.HandleWebhookPost(raw);
                if (response is not null) // ... if the clients response is not null ...
                    return Ok(JsonConvert.SerializeObject(response)); // ... serialize it and send it.
                else return BadRequest("Failed to parse request JSON."); // ... or send a bad request message.
            }
        }
    }
}
