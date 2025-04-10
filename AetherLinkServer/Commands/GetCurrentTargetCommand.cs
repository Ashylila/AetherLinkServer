using AetherLinkServer.DalamudServices;
using AetherLinkServer.Models;
using System.Threading.Tasks;
using AetherLinkServer.Attributes;
using AetherLinkServer.Handlers;
using AetherLinkServer.Services;
using AetherLinkServer.Utility;
using Dalamud.Game.ClientState.Objects;
using ECommons.Reflection;

namespace AetherLinkServer.Commands;

public class GetCurrentTargetCommand : CommandBase
{
    private readonly ITargetManager _target;
    public GetCurrentTargetCommand(ITargetManager target)
    {
        _target = target;
    }
    [Command("getcurrenttarget", "Get the current target's name and ID")]
    public async Task Execute(string args)
    {
        var target = _target.Target;
        if (target == null)
        {
            await SendCommandResponse("Target not found", CommandResponseType.Error);
            return;
        }
        
        await SendCommandResponse($"Current target: {target.Name} ({target.DataId})", CommandResponseType.Success);
    }
}
