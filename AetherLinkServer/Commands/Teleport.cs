using System;
using System.Threading;
using System.Net.WebSockets;
using AetherLinkServer.Utility;
using AetherLinkServer.Models;
using ECommons.Automation;
using System.Threading.Tasks;
namespace AetherLinkServer.Commands;

public class TeleportCommand : Models.ICommand
{
    public string Name => "teleport";
    public void Execute(string args)
    {
    Chat.Instance.ExecuteCommand($"/tp {args}");
    Task.Run(async () => Plugin.server.SendMessage(CommandHelper.createCommand("Teleporting...", WebSocketActionType.CommandResponse)));
    }
}