using System;
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.SlashCommands.Entities
{
    public class SlashCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? CommandId { get; set; }
        public ulong? ApplicationId { get; set; }
        public int Version { get; set; }
        public SlashSubcommand? Command { get; init; }

        public Dictionary<string, SlashSubcommandGroup>? Subcommands { get; init; }

        public SlashCommand(string name, int version, SlashSubcommand command, ulong? gid)
        {
            Name = name;
            Version = version;
            Description = command.Description;
            GuildId = gid;
            Command = command;
            Subcommands = null;
        }

        public SlashCommand(string name, int version, SlashSubcommandGroup[] subcommands, ulong? gid, string desc = "n/a")
        {
            Name = name;
            Version = version;
            Description = desc;
            Subcommands = subcommands.ToDictionary(x => x.Name);
            GuildId = gid;
            Command = null;
        }

        /// <summary>
        /// Attempts to execute a command from a command with no subcommands.
        /// </summary>
        /// <param name="args">Command arguments</param>
        /// <returns>True if the command was attempted, false if there was no command to attempt.</returns>
        public bool ExecuteCommand(BaseDiscordClient c, InteractionContext ctx, params object[] args)
        {
            List<object> combinedArgs = new List<object>();
            combinedArgs.Add(ctx);
            combinedArgs.AddRange(args);

            var cArgs = combinedArgs.ToArray();

            if (Command is not null)
            {
                Command.ExecuteCommand(c, ctx.Interaction.GuildId, cArgs);
                return true;
            }
            else 
            {
                if(Subcommands is null) return false;

                var group = ctx.Interaction.Data?.Options?.FirstOrDefault();
                if(group is not null)
                {
                    if(Subcommands.TryGetValue(group.Name, out var cmdGroup))
                    {
                        var cmdData = group.Options?.FirstOrDefault();
                        if(cmdData is not null)
                        {
                            if(cmdGroup.Commands.TryGetValue(cmdData.Name, out var cmd))
                            {
                                cmd.ExecuteCommand(c, ctx.Interaction.GuildId, cArgs);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public SlashCommandConfiguration GetConfiguration()
        {
            return new SlashCommandConfiguration()
            {
                CommandId = CommandId ?? throw new Exception("Failed to get a valid command ID"),
                GuildId = GuildId,
                Name = Name,
                Version = Version
            };
        }
    }
}
