using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Enums;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace ExampleGatewayBot
{
    class Program
    {
        public static DiscordClient Discord;
        public static DiscordSlashClient Slash;
        static void Main(string[] args)
        {
            // Read the config json file ...
            using FileStream fs = new(Path.Join("Config", "bot_config.json"), FileMode.Open);
            using StreamReader sr = new(fs);
            var json = sr.ReadToEnd();

            var jobj = JObject.Parse(json);
            // ... create a new DiscordClient for the bot ...
            Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = jobj["token"].ToString(),
                TokenType = TokenType.Bot,
                ShardCount = 1,
                Intents = DiscordIntents.AllUnprivileged,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            });

            // ... register commands ...
            var next = Discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { jobj["prefix"].ToString() }
            });

            next.RegisterCommands(Assembly.GetExecutingAssembly());
            // ... register the interaction event ...
            Discord.InteractionCreated += Discord_InteractionCreated;
            Discord.ApplicationCommandCreated += Discord_ApplicationCommandCreated;
            Discord.ApplicationCommandDeleted += Discord_ApplicationCommandDeleted;
            Discord.ApplicationCommandUpdated += Discord_ApplicationCommandUpdated;

            // ... connect to discord ...
            Discord.ConnectAsync().GetAwaiter().GetResult();

            var defaultResponseData = new InteractionApplicationCommandCallbackDataBuilder()
                .WithContent("`Test Automated Response`");

            // ... use the discord connection to build the Slash Client config ...
            Slash = new DiscordSlashClient(new DiscordSlashConfiguration
            {
                Client = Discord,
                Token = jobj["token"].ToString(),
                DefaultResponseType = InteractionResponseType.ChannelMessageWithSource,
                DefaultResponseData = defaultResponseData
            });

            Slash.RegisterCommands(Assembly.GetExecutingAssembly());

            // ... start the slash client ...
            Slash.StartAsync().GetAwaiter().GetResult();

            // ... and prevent this from stopping.
            Task.Delay(-1).GetAwaiter().GetResult();
        }

        private static Task Discord_ApplicationCommandUpdated(DiscordClient sender, DSharpPlus.EventArgs.ApplicationCommandEventArgs e)
        {
            Discord.Logger.LogInformation($"Shard {sender.ShardId} sent application command updated: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandDeleted(DiscordClient sender, DSharpPlus.EventArgs.ApplicationCommandEventArgs e)
        {
            Discord.Logger.LogInformation($"Shard {sender.ShardId} sent application command deleted: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandCreated(DiscordClient sender, DSharpPlus.EventArgs.ApplicationCommandEventArgs e)
        {
            Discord.Logger.LogInformation($"Shard {sender.ShardId} sent application command created: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static async Task Discord_InteractionCreated(DiscordClient sender, DSharpPlus.EventArgs.InteractionCreateEventArgs e)
            => await Slash.HandleGatewayEvent(e);
    }
}
