using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Enums;

namespace ExampleBot.Commands.Slash
{
    public class HelloWorldSlashCommand : SlashCommandBase
    {
        public HelloWorldSlashCommand(IServiceProvider proivder) : base(proivder) { }

        [SlashCommand("hello", 1, 750486424469372970)]
        public async Task HelloWorldSlashCommandAsync(InteractionContext ctx)
        {
            var response = new InteractionResponseBuilder()
                .WithType(InteractionResponseType.ChannelMessage)
                .WithData(new InteractionApplicationCommandCallbackDataBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Hello World!")
                        .WithDescription($"And hello to you too, {ctx.Interaction.User.Username}"))
                    .WithContent("How's Life?"));

            await ctx.ReplyAsync(response.Build());
        }
    }
}
