# DSharpPlus.SlashCommands
A SlashCommand implementation for DSharpPlus. This does not connect to the gateway and gateway events or HTTP post events must be handled by the client.

# Notices
### While commands will be added to Discord, not all limit checks are done by this utility. Please make sure you are not violating them yourself by checking the limits [here](#command-limits).
### Standard DI is not implemented, the IServiceProvider is how you can get services as of right now
### You cant add services to the IServiceProvider at the moment, but it does have a logger for logging.

# Quickstart
> The ExampleBot project is a working example on how to use SlashCommands with a DiscordClient from DSharpPlus.

## Install the Lib
**Nuget Package:** `Soyvolon.DSharpPlus.SlashCommands`
## Creating a new Project
As this lib currently only supports Webhook interaction, make sure to create an ASP.NET Core Web application. It is recommended to use an ASP.NET Core Web API if nothing else will be created on the web app.

## Create the `DiscordSlashClient`
A Discord Slash Client requires two things:
1. A Bot token for verification with Discord
2. An active Discord Client. Use `Client =` for a `DiscordClient` and `DiscordRestClient`, and `ShardedClient =` for a `DiscordShardedClient`.

The Token and Application ID need to be from the same application.

First, create a new DiscordSlashConfiguration:
```csharp
var config = new DiscordSlashConfiguration
{
    Client = <Discord Client or Discord Rest Client>,
    Token = <bot token>
}
```
Or by using a ShardedClient instead:
```csharp
var config = new DiscordSlashConfiguration
{
    ShardedClient = <Discord Sharded Client>,
    Token = <bot token>
}
```

Then, just like in DSharpPlus, pass the Configuration into the constructor for the `DiscordSlashClient` to get your client object:
```csharp
var client = new DiscordSlashClient(config);
```

You can customize the `DiscordSlashClient`'s custom responses with additional options in the configuration. For Example:
```csharp
DefaultResponseType = InteractionResponseType.ChannelMessageWithSource,
DefaultResponseData = new InteractionApplicationCommandCallbackDataBuilder()
```

## Adding Commands
Commands are created similarly to in `DSharpPlus.CommandsNext`. Some of the attributes that are looked for in commands are taken from `CommandsNext`.

A command with no subcommands can be created like this:
```csharp
public class HelloWorldSlashCommand : BaseSlashCommandModule
{
    public HelloWorldSlashCommand(IServiceProvider provider) : base(provider) { }

    [SlashCommand("hello", 1, 750486424469372970)]
    public async Task HelloWorldSlashCommandAsync(InteractionContext ctx)
    {
        // Command Code here.
    }
}
```
This creates the first version of the hello command, for the guild `750486424469372970`. If you want a global command, leave the guild out of the attribute:
```csharp
[SlashCommand("hello", 1)]
```

> A Commands version number is used to tell the Lib when to send an update to discord. If you change the parameters of a command, update the version number or the command will not be updated with Discord.

From there, you need to tell the library what Assemblies to look for commands in:
```csharp
client.RegisterCommands(Assembly.GetExecutingAssembly());
```
This will get all commands that are in the same Assembly as the executing assembly and register them with the Library.

For how to create subcommands, see [Creating Subcommands](#creating-subcommands)

## Starting the Slash Client
Starting the client is simple, just run:
```csharp
await client.StartAsync();
```
After you have registered commands.

As the client starts, it will build a JSON file inside the executing assembly. This JSON file is needed to tell the Library what commands have already been registered with Discord. It is named `sccfg_<client ID>.json`.
> Deleting the JSON file can case unexpected command behavior where commands don't get deleted when they are supposed to, or commands are not updated correctly after version numbers update.

> This JSON file is for a single application only, running the same client on two different applications can cause unexpected behavior as well.

## Handling Incoming Webhooks
Now that the `DiscordSlashClient` is running, you need to handle incoming webhooks from Discord.

Create a new `Controller` in your ASP.NET Core project. In this example, we also get the ASP.NET Core logger to log events in the API:
```csharp
[Route("api/discordslash")]
[ApiController]
public class DiscordSlashCommandController : ControllerBase
{
    private readonly ILogger _logger;

    public DiscordSlashCommandController(ILogger<DiscordSlashCommandController> logger)
    {
        _logger = logger;
    }
}
```
The `[Route("api/discordslash")]` attribute determines where the program needs to listen for incoming requests. In this case, we are listening at `https://slash.example.com/api/discordslash` for incoming requests.

The `[ApiController]` attribute tells ASP.NET that this class is apart of our API.

Discord will send `POST` requests, so lets build a method to handle those within our class:
```csharp
[HttpPost("")]
//[ApiExplorerSettings(IgnoreApi = true)]
public async Task<IActionResult> DiscordEndpointHandler()
{
    
}
```
The `[HttpPost("")]` attribute tells ASP.NET to run this method when a `POST` request comes to the default route, or `/api/discordslash`.

For Webhooks, you need to validate the request and response with a `401 Unauthorized` if it is a bad request as per [Discord Docs](https://discord.com/developers/docs/interactions/slash-commands#security-and-authorization)

With that in mind, our example program is using [Sodium Core](https://github.com/tabrath/libsodium-core/) to validate our responses, along with some helper code form [StackOverflow](https://stackoverflow.com/a/5919521/11682098) that parses the Hex tokens to a `byte[]`.

The Util Class:
```csharp
public static class Utils
{
    public static readonly int[] HexValue = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
    0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

    // Code Snippet from: https://stackoverflow.com/a/5919521/11682098
    // Code Snippet by: Nathan Moinvaziri
    public static byte[] HexStringToByteArray(string Hex)
    {
        byte[] Bytes = new byte[Hex.Length / 2];

        for (int x = 0, i = 0; i < Hex.Length; i += 2, x += 1)
        {
            Bytes[x] = (byte)(HexValue[Char.ToUpper(Hex[i + 0]) - '0'] << 4 |
                                HexValue[Char.ToUpper(Hex[i + 1]) - '0']);
        }

        return Bytes;
    }
}
```
Request Validation (Goes inside the `DiscordEndpointHandler` method):
```csharp
string raw;
// Request validation
try
{
    // Get the verification headers from the request ...
    var signature = Request.Headers["X-Signature-Ed25519"].ToString();
    var timestamp = Request.Headers["X-Signature-Timestamp"].ToString();
    // ... convert the signature and public key to byte[] to use in verification ...
    var byteSig = Utils.HexStringToByteArray(signature);
    // NOTE: This reads your Public Key that you need to store somewhere.
    var byteKey = Utils.HexStringToByteArray(PublicKey);
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
```
> The body of the request is stored in the `string raw;` variable that is defined before the try catch block so it can be used in the next part.

As explained in the code comments, this snippet does the following:
1. Get the request headers from the incoming request.
2. Converts the request signature to a `byte[]`.
3. Converts your application's public key (see discord developers page for your application to obtain this) into a `byte[]`.
4. Combines the timestamp and request body, and parses it into a `byte[]`.
5. Takes the three `byte[]`s and uses Sodium Core to validate the request.
6. If the code is invalid, returns the required `401 Unauthorized`, otherwise continues onward.

After the request is validated, we can parse the request into either a `PONG` response, or pass it to the `DiscordSlashClient` which will return the default response for us to send back to Discord.

In this case, we first attempt to parse the body into a `JObject` so we can see if this is a `PING`:
```csharp
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
```
Once that is done, we test if it is a `PING` and response accordingly:
```csharp
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
```
Otherwise, we send this to the `DiscordSlashClient` to be handled `async`:
```csharp
else
{// ... then pass the raw request body to the client ...
    var response = await client.HandleWebhookPost(raw);
    if (response is not null) // ... if the clients response is not null ...
        return Ok(JsonConvert.SerializeObject(response)); // ... serialize it and send it.
    else return BadRequest("Failed to parse request JSON."); // ... or send a bad request message.
}
```
The full Controller class looks like this:
```csharp
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
```
## Telling Discord to send you Interactions over webhooks
In order for everything to work, Discord needs to know to send you information over Webhook and not the Gateway. This means you need at least a development version of the bot running on the server you intend to release it to.

Once the bot is running, and your API is ready to receive requests, head over to your discord developer portal and select your application. In the General Information tab, near the bottom there is an Interactions Endpoint URL field. Input your API endpoint there. For example, using the URL that was used earlier our endpoint would be: `https://slash.example.com/api/discordslash`

Once you hit save, Discord is going to send a `POST` request to your URL (thus why it needs to be port-forwarded or on a server). This is where the Ping response comes in. Your app will recognize the Ping, respond with Pong, and Discord will save your endpoint.

> **Congrats, you now have SlashCommands setup!** <br />
*Example code was from the ExampleBot project.*

# Further Options
## Creating Subcommands
Due to how Discord has setup the commands for Slash Interactions, the setup used when creating Subcommands and Subcommand groups is a little wonky.

See the [Discord Docs](https://discord.com/developers/docs/interactions/slash-commands#subcommands-and-subcommand-groups) for more information.

Firstly, you need your command class:
```csharp
[SlashCommand("sub", 1, 750486424469372970)]
public class SubcommandExampleSlashCommand : BaseSlashCommandModule
{
    // NOTE: THis way of DI will change at some point when I get around to making it actual DI
    public SubcommandExampleSlashCommand(IServiceProvider p) : base(p) { }
}
```
Then, you need a child class of that command class:
```csharp
[SlashSubcommandGroup("group")]
public class SubcommandGroup : SubcommandExampleSlashCommand
{
    public SubcommandGroup(IServiceProvider p) : base(p) { }

    // command methods go here.
}
```
And finally, the command method:
```csharp
[SlashSubcommand("command")]
public async Task CommandAsync(InteractionContext ctx)
{
    await ctx.ReplyAsync("This is a subcommand");
}
```

Once done, your file should look a bit like this:
```csharp
using System;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;

namespace ExampleBot.Commands.Slash
{
    [SlashCommand("sub", 1, 750486424469372970)]
    public class SubcommandExampleSlashCommand : BaseSlashCommandModule
    {
        // NOTE: This way of DI will change at some point when I get around to making it actual DI
        public SubcommandExampleSlashCommand(IServiceProvider p) : base(p) { }
    }

    [SlashSubcommandGroup("group")]
    public class SubcommandGroup : SubcommandExampleSlashCommand
    {
        public SubcommandGroup(IServiceProvider p) : base(p) { }

        [SlashSubcommand("command")]
        public async Task CommandAsync(InteractionContext ctx)
        {
            await ctx.ReplyAsync("This is a subcommand");
        }
    }
}
```
This creates a command named `sub` with a subcommand group called `group` and a subcommand named `command`. It is called from discord like so: `/sub group command`

A few things to note:
- If you have a subcommand, you can not have a default command.
- There is a max of 10 subcommand groups per command
- There is a max of 10 subcommands per subcommand group.

## Command Limits
Some rules to keep in mind for adding parameters:

*From the [Discord Docs](https://discord.com/developers/docs/interactions/slash-commands#a-quick-note-on-limits)*
- An app can have up to 50 top-level global commands (50 commands with unique names)
- An app can have up to an additional 50 guild commands per guild
- An app can have up to 10 subcommand groups on a top-level command
- An app can have up to 10 subcommands within a subcommand group
- `choices` can have up to 10 values per option
- commands can have up to 10 options per command
- Limitations on [command names](https://discord.com/developers/docs/interactions/slash-commands#registering-a-command)
- Limitations on [nesting subcommands and groups](https://discord.com/developers/docs/interactions/slash-commands#nested-subcommands-and-groups)
### Using Enums as Parameters for Commands
You can use an `enum` as a parameter for a command. However, due to the limits Discord sets, you can only have up to ten values for your `enum`.

> An enum is automatically assigned the `choices` type in the Discord Slash command.

To set a description for your `enum` value, use the `System.ComponentModel.DescriptionAttribute` over the `DSharpPlus` version, as the `DSharpPlus` description attribute does not work on `enum` values.