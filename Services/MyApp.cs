using Commands;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading.Tasks;

namespace Services
{
    internal class MyApp
    {
        private readonly IEnumerable<ICommandHandlerBuilder> _commandBuilders;

        public MyApp(IEnumerable<ICommandHandlerBuilder> commandBuilders)
        {
            _commandBuilders = commandBuilders;
        }

        public async Task<int> RunAsync(string[] args)
        {
            var cmd = new RootCommand();
            
            foreach (var commandBuilder in _commandBuilders)
            {
                cmd.AddCommand(commandBuilder.Build());
            }

            return await cmd.InvokeAsync(args);
        }
    }
}