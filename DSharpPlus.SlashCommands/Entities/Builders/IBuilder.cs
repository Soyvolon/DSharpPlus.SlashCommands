using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.Entities.Builders
{
    public interface IBuilder<T>
    {
        public T Build();
    }
}
