using System.Threading.Tasks;
using AetherLinkServer.Models;
using AetherLinkServer.Services;
using AetherLinkServer.Utility;

namespace AetherLinkServer.Handlers;

public abstract class CommandBase : ICommand
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    
    public CommandContext Context { get; private set; } = null!;
    
    internal void SetContext(CommandContext context)
    {
        Context = context;
    }

    protected async Task SendCommandResponse(string response, CommandResponseType responseType = CommandResponseType.Info)
    {
        await Context.Server.SendCommandResponse(response, responseType);
    }
    public abstract Task Execute(string args);
}
