using System;
using System.Linq;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Handlers;
using AetherLinkServer.Models;
using AetherLinkServer.Services;
using AetherLinkServer.Utility;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace AetherLinkServer.Commands;

public class TeleportCommand : CommandBase
{
    private readonly IClientState _client;
    public override string Name => "teleport";
    public override string Description => "Teleport to a location using the AetherLink server.";
    public TeleportCommand(IClientState client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }
    public override async Task Execute(string args)
    {
        if (_client.LocalPlayer == null)
        {
            await SendCommandResponse("Player is busy or not in-game.", CommandResponseType.Error);
            return;
        }
        
        var location = TeleportHelper.TryFindAetheryteByName(args, out var info, out var aetheryte);
        if (!location)
        {
            await SendCommandResponse("Location not found.", CommandResponseType.Error);
            return;
        }
        var result = TeleportHelper.Teleport(info.AetheryteId, info.SubIndex);
        if (!result)
        {
            await SendCommandResponse("Failed to teleport.", CommandResponseType.Error);
            return;
        }
        
        await SendCommandResponse($"Teleporting to {aetheryte}...", CommandResponseType.Success);
    }
}
