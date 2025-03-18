using System.Windows.Input;
using AetherLinkServer.IPC;
using ECommons.Automation;
namespace AetherLinkServer.Commands;

public class TeleportCommand : Models.ICommand
{
    public string Name => "teleport";
    public void Execute(string args)
    {
    Chat.Instance.ExecuteCommand($"/tp {args}");
    }
}