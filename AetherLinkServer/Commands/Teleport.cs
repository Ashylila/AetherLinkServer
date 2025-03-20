using System;
using System.Threading;
using System.Net.WebSockets;
using AetherLinkServer.Utility;
using AetherLinkServer.Models;
using ECommons.Automation;
using System.Threading.Tasks;
using ECommons.GameHelpers;
using ECommons.GameFunctions;
namespace AetherLinkServer.Commands;

public class TeleportCommand : Models.ICommand
{
    public string Name => "teleport";
    public async Task Execute(string args)
    {
    if(!Player.Available)
    {
        await CommandHelper.SendCommand("You must be in-game to use this command.", WebSocketActionType.CommandResponse);
        return;
    }
    Chat.Instance.ExecuteCommand($"/tp {args}");
    await CommandHelper.SendCommand("Teleporting...", WebSocketActionType.CommandResponse);
    }
}