﻿using System;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;

namespace ExampleBot.Commands.Slash
{
    [SlashCommand("sub", 750486424469372970)]
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
