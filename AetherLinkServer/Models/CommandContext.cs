using System.Threading.Tasks;
using AetherLinkServer.Models;
using AetherLinkServer.Services;
using AetherLinkServer.Utility;

namespace AetherLinkServer.Handlers;

public class CommandContext
{
    public required WebSocketServer Server { get; init; } 
    
    public string? CommandName { get; init; }
    public string? Description { get; init; }
    
    public async Task SendCommandResponse(string message, CommandResponseType type)
    {
        await Server.SendCommandResponse(message, type);
    }
}
