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
    public class ArgumentExampleCommand : BaseSlashCommandModule
    {
        TestService service;

        public ArgumentExampleCommand(IServiceProvider provider) : base(provider) 
        {
            service = provider.GetService<TestService>();
        }

        [SlashCommand("args", 1, 750486424469372970)]
        public async Task ArgumentExampleCommandAsync(InteractionContext ctx, TestChoices choice, int age, string name, bool female,
            DiscordUser user, DiscordChannel channel, DiscordRole role)
        {
            var response = new InteractionResponseBuilder()
                .WithType(InteractionResponseType.ChannelMessageWithSource)
                .WithData(new InteractionApplicationCommandCallbackDataBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Testing Arguments!")
                        .WithDescription($"Choice: {choice}\n" +
                        $"Age: {age}\n" +
                        $"Name: {name}\n" +
                        $"Female? {female}\n" +
                        $"User: {user.Username}\n" +
                        $"Channel: {channel.Name}\n" +
                        $"Role: {role.Name}"))
                    .WithContent("How's Life?"));

            await ctx.ReplyAsync(response.Build());
        }

        public enum TestChoices
        {
            Happy = 2,
            Sad,
            Quiet,
            Tall,
            Short
        }
    }
}
