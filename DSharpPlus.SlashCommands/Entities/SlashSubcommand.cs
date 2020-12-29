using System;
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
            var parsedArgs = ParseArguments(args);
            ExecutionMethod.Invoke(BaseCommand, parsedArgs);

            return Task.CompletedTask;
        }

        private object[] ParseArguments(object[] args)
        {
            var parsedArgs = new object[args.Length];
            var parameters = ExecutionMethod.GetParameters();

            for(int i = 0; i < args.Length; i++)
            {
                var param = parameters[i];
                
                if(param.ParameterType.IsEnum)
                {
                    var e = ParseEnum(args[i], param);

                    if (e is null)
                        throw new ArgumentNullException(param.Name, "Failed to parse Discord result to enum value.");

                    parsedArgs[i] = e;
                }
                else
                {
                    parsedArgs[i] = args[i];
                }
            }

            return parsedArgs;
        }

        private object? ParseEnum(object arg, ParameterInfo info)
        {
            if (int.TryParse(arg as string, out var i))
            {
                var e = Enum.ToObject(info.ParameterType, i);
                return e;
            }

            return null;
        }
    }
}
