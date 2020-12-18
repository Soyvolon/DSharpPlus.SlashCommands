using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace ExampleBot.Commands.Discord
{
    public class PingDiscordCommand : BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            Stopwatch timer = new Stopwatch();
            var pingEmbed = new DiscordEmbedBuilder().WithColor(DiscordColor.CornflowerBlue).WithTitle($"Ping for Shard {ctx.Client.ShardId}");
            pingEmbed.AddField("WS Latency:", $"{ctx.Client.Ping}ms");
            timer.Start();
            DiscordMessage msg = await ctx.RespondAsync(null, false, pingEmbed);
            await msg.ModifyAsync(null, pingEmbed.AddField("Response Time: (:ping_pong:)", $"{timer.ElapsedMilliseconds}ms").Build());
            timer.Stop();
        }
    }
}
