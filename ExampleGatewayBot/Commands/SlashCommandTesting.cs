using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Entities.Builders;
using DSharpPlus.SlashCommands.Enums;

namespace ExampleGatewayBot.Commands
{
    public class SlashCommandTesting : BaseSlashCommandModule
    {
        public SlashCommandTesting(IServiceProvider p) : base(p) { }

        [SlashCommand("ping", 1, 431462786900688896)]
        public async Task SlashCommandTestingAsync(InteractionContext ctx)
            => await ctx.ReplyAsync($"Pong: {Program.Discord.Ping}");

        [SlashCommand("say", 1, 431462786900688896)]
        public async Task SlashCommandTestingTwoAsync(InteractionContext ctx, string toSay)
            => await ctx.ReplyAsync(toSay);

        [SlashCommand("add", 1, 431462786900688896)]
        public async Task MathCommandAsync(InteractionContext ctx, int num1, int num2)
            => await ctx.ReplyAsync($"{num1 + num2}");

        [SlashCommand("subtract", 1, 431462786900688896)]
        public async Task SubtractCommandAsync(InteractionContext ctx, int num1, int num3)
            => await ctx.ReplyAsync($"{num1 - num3}");
    }

    [SlashCommand("subs", 1, 431462786900688896)]
    public class ArgumentSubcommandCommand : BaseSlashCommandModule
    {

        public ArgumentSubcommandCommand(IServiceProvider provider) : base(provider)
        {

        }
    }

    [SlashSubcommandGroup("params")]
    public class ArgumentSubcommandCommandGroup : ArgumentSubcommandCommand
    {

        public ArgumentSubcommandCommandGroup(IServiceProvider provider) : base(provider)
        {

        }

        [SlashSubcommand("test")]
        public async Task ArgumentSubcommandCommandAsync(InteractionContext ctx, TestChoices choice, int age, string name, bool female,
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
