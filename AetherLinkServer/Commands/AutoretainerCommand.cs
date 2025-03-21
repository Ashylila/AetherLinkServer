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
    public async Task Execute(string args)
    {
        if(args == string.Empty)
        {
            string error = "No arguments provided for command Autoretainer. See 'autoretainer help' for more information.";
            await CommandHelper.SendCommandResponse(error, CommandResponseType.Warning);
        }
    }
}