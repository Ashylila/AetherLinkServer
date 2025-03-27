using System;
using System.Linq;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Models;
using AetherLinkServer.Utility;
using Lumina.Excel.Sheets;

namespace AetherLinkServer.Commands;

public class TeleportCommand : ICommand
{
    public string Name => "teleport";

    public async Task Execute(string args)
    {
        if (Svc.ClientState.LocalPlayer == null)
        {
            await CommandHelper.SendCommand("Player is busy or not in-game.", WebSocketActionType.CommandResponse);
            return;
        }

        var location = TeleportHelper.TryFindAetheryteByName(args, out var info, out var aetheryte);
        if (!location)
        {
            await CommandHelper.SendCommandResponse("Location not found.", CommandResponseType.Error);
            return;
        }

        var result = TeleportHelper.Teleport(info.AetheryteId, info.SubIndex);
        if (!result) await CommandHelper.SendCommandResponse("Failed to teleport.", CommandResponseType.Error);
        //Chat.Instance.ExecuteCommand($"/tp {args}");
        var name = Svc.Data.GetExcelSheet<Aetheryte>()
                      .FirstOrDefault(t => t.PlaceName.Value.Name.ToString()
                                            .Contains(args, StringComparison.OrdinalIgnoreCase)).PlaceName.Value.Name
                      .ToString();
        await CommandHelper.SendCommandResponse($"Teleporting to {aetheryte}...", CommandResponseType.Info);
    }
}
