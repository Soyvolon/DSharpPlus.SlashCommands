using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace ExampleBot
{
    public class Program
    {
        public static DiscordClient Discord { get; private set; }
        public static DiscordSlashClient Slash { get; private set; }

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        public class TestService
        {
            
        }

        public static async Task MainAsync(string[] args)
        {
            // Read the config json file ...
            using FileStream fs = new(Path.Join("Config", "bot_config.json"), FileMode.Open);
            using StreamReader sr = new(fs);
            var json = await sr.ReadToEndAsync();

            var jobj = JObject.Parse(json);
            // ... create a new DiscordClient for the bot ...
            Discord = new DiscordClient(new DiscordConfiguration
            { 
                Token = jobj["token"].ToString(),
                TokenType = TokenType.Bot,
                ShardCount = 1,
                Intents = DiscordIntents.AllUnprivileged
            });
            // ... register commands ...
            var next = Discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { jobj["prefix"].ToString() }
            });

            next.RegisterCommands(Assembly.GetExecutingAssembly());
            // ... connect to discord ...
            await Discord.ConnectAsync();

            var defaultResponseData = new InteractionApplicationCommandCallbackDataBuilder()
                .WithContent("`Test Automated Response`");

            IServiceCollection c = new ServiceCollection();
            c.AddTransient<TestService>();

            // ... use the discord connection to build the Slash Client config ...
            Slash = new DiscordSlashClient(new DiscordSlashConfiguration
            {
                ClientId = Discord.CurrentApplication.Id,
                Token = jobj["token"].ToString(),
                DefaultResponseType = InteractionResponseType.ChannelMessageWithSource,
                DefaultResponseData = defaultResponseData
            }, c);

            Slash.RegisterCommands(Assembly.GetExecutingAssembly());

            // ... start the slash client ...
            await Slash.StartAsync();
            // ... build the web server for receiving HTTP POSTs from discord ...
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
            // ... and start the webserver ...
            await host.Build().StartAsync();
            // ... then hold here to prevent premature closing.
            await Task.Delay(-1);
        }
    }
}
