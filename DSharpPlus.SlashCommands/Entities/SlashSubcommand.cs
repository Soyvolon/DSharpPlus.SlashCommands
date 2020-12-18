using System.Reflection;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.Entities
{
    public class SlashSubcommand
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public MethodInfo ExecutionMethod { get; init; }
        public BaseSlashCommandModule BaseCommand { get; init; }

        public SlashSubcommand(string name, string desc, MethodInfo method, BaseSlashCommandModule commandInstance)
        {
            Name = name;
            Description = desc;
            ExecutionMethod = method;
            BaseCommand = commandInstance;
        }

        public Task ExecuteCommand(params object[] args)
        {
            ExecutionMethod.Invoke(BaseCommand, args);

            return Task.CompletedTask;
        }
    }
}
