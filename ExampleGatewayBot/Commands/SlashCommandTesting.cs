using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;

namespace ExampleGatewayBot.Commands
{
    public class SlashCommandTesting : BaseSlashCommandModule
    {
        public SlashCommandTesting(IServiceProvider p) : base(p) { }

        [SlashCommand("ping", 1, 431462786900688896)]
        public async Task SlashCommandTestingAsync(InteractionContext ctx)
        {
            await ctx.ReplyAsync($"Pong: {Program.Discord.Ping}");
        }
    }
}
