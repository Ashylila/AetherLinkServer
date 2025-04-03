using AetherLinkServer.DalamudServices;
using AetherLinkServer.Models;
using System.Threading.Tasks;
using AetherLinkServer.Utility;
using ECommons.Reflection;

namespace AetherLinkServer.Commands;

public class GetCurrentTargetCommand : ICommand
{
    public string Name => "getcurrenttarget";
    public async Task Execute(string args, Plugin plugin)
    {
        var target = Svc.Targets.Target;
        if (target == null)
        {
            await CommandHelper.SendCommandResponse("Target not found", CommandResponseType.Error);
            return;
        }
        
        await CommandHelper.SendCommandResponse($"Current target: {target.Name} ({target.DataId})", CommandResponseType.Success);
    }
}
