using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Enums;

using Microsoft.Extensions.DependencyInjection;

using static ExampleBot.Program;

namespace ExampleBot.Commands.Slash
{
    public class HelloWorldSlashCommand : BaseSlashCommandModule
    {
        TestService service;

        public HelloWorldSlashCommand(IServiceProvider provider) : base(provider) 
        {
            service = provider.GetService<TestService>();
        }

        [SlashCommand("hello", 750486424469372970)]
        public async Task HelloWorldSlashCommandAsync(InteractionContext ctx)
        {
            var response = new InteractionResponseBuilder()
                .WithType(InteractionResponseType.ChannelMessageWithSource)
                .WithData(new InteractionApplicationCommandCallbackDataBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Hello World!")
                        .WithDescription($"And hello to you too, {ctx.Interaction.User.Username}"))
                    .WithContent("How's Life?"));

            await ctx.ReplyAsync(response.Build());
        }
    }
}
