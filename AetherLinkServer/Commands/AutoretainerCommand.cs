using System.Threading.Tasks;
using AetherLinkServer.Models;
using AetherLinkServer.Utility;
using System.Threading;
using System;
using System.Net.WebSockets;

namespace AetherLinkServer.Commands;

public class AutoretainerCommand : ICommand
{
    public string Name => "autoretainer";
    public void Execute(string args)
    {
        if(args == string.Empty)
        {
            string error = "No arguments provided for command Autoretainer. See 'autoretainer help' for more information.";
            Task.Run(async () => CommandHelper.SendCommand(error, WebSocketActionType.CommandResponse));
        }
    }
}