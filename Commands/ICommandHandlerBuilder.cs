using System.CommandLine;

namespace Commands
{
    interface ICommandHandlerBuilder
    {
        Command Build();
    }
}