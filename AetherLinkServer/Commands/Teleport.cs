using System;
using System.Threading;
using System.Net.WebSockets;
using AetherLinkServer.Utility;
using AetherLinkServer.Models;
using ECommons.Automation;
using System.Threading.Tasks;
using ECommons.GameHelpers;
using ECommons.GameFunctions;
using AetherLinkServer.DalamudServices;
using Lumina.Excel.Sheets;
using ECommons;
using System.Linq;
using AetherLinkServer.IPC;
namespace AetherLinkServer.Commands;

public class TeleportCommand : Models.ICommand
{
    public string Name => "teleport";
    public async Task Execute(string args) 
    {

        if(Svc.ClientState.LocalPlayer == null) 
    { 
        await CommandHelper.SendCommand("Player is busy or not in-game.", WebSocketActionType.CommandResponse);
        return; 
    }
    var location = TeleportHelper.TryFindAetheryteByName(args, out var info, out var aetheryte);
    if(!location)
    {
        await CommandHelper.SendCommandResponse("Location not found.", CommandResponseType.Error);
        return;
    }
    var result = TeleportHelper.Teleport(info.AetheryteId, info.SubIndex);
    if (!result)
    {
        await CommandHelper.SendCommandResponse("Failed to teleport.", CommandResponseType.Error);
    }
    //Chat.Instance.ExecuteCommand($"/tp {args}");
    string name = Svc.Data.GetExcelSheet<Aetheryte>().FirstOrDefault(t => t.PlaceName.Value.Name.ToString().Contains(args,StringComparison.OrdinalIgnoreCase)).PlaceName.Value.Name.ToString();
    await CommandHelper.SendCommandResponse($"Teleporting to {aetheryte}...", CommandResponseType.Info);
    }
}
