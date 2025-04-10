using System;
using System.Linq;
using System.Threading.Tasks;
using AetherLinkServer.Attributes;
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

    public TeleportCommand(IClientState client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    [Command("teleport", "Teleport to a location by name.")]
    public async Task Execute(string args)
    {
        try
        {
            // Check if _client or LocalPlayer is null
            if (_client == null)
            {
                await SendCommandResponse("Client state is null. Unable to execute teleport command.",
                                          CommandResponseType.Error);
                return;
            }

            if (_client.LocalPlayer == null)
            {
                await SendCommandResponse("Player is busy or not in-game.", CommandResponseType.Error);
                return;
            }

            // Try to find the location
            var location = TeleportHelper.TryFindAetheryteByName(args, out var info, out var aetheryte);
            if (!location)
            {
                await SendCommandResponse("Location not found.", CommandResponseType.Error);
                return;
            }

            // Try to teleport
            var result = TeleportHelper.Teleport(info.AetheryteId, info.SubIndex);
            if (!result)
            {
                await SendCommandResponse("Failed to teleport.", CommandResponseType.Error);
                return;
            }

            // Success
            await SendCommandResponse($"Teleporting to {aetheryte}...", CommandResponseType.Success);
        }
        catch (Exception ex)
        {
            // General exception handling for logging
            await SendCommandResponse($"An unexpected error occurred: {ex.Message}", CommandResponseType.Error);
        }
    }
}
