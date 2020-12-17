﻿using System;
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

                var reader = new StreamReader(Request.Body);
                if (reader.BaseStream.CanSeek)
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                raw = await reader.ReadToEndAsync();
                
                var byteBody = Encoding.UTF8.GetBytes(timestamp + raw);

                var miniKey = Minisign.Core.LoadPublicKeyFromString(Startup.PublicKey);
                var miniSig = Minisign.Core.LoadSignatureFromString(signature, "Ed25519", "Ed25519");

                var validated = Minisign.Core.ValidateSignature(byteBody, miniSig, miniKey);

                if(!validated)
                {
                    _logger.LogWarning("Failed to validate POST request from Discord.");
                    return Unauthorized("Invalid Request Signature");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Decryption failed.");
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
