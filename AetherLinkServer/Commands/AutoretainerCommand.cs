using System.Threading.Tasks;
using AetherLinkServer.Models;
using AetherLinkServer.Utility;

namespace AetherLinkServer.Commands;

public class AutoretainerCommand : ICommand
{
    public string Name => "autoretainer";

    public async Task Execute(string args)
    {
        if (args == string.Empty)
        {
            var error = "No arguments provided for command Autoretainer. See 'autoretainer help' for more information.";
            await CommandHelper.SendCommandResponse(error, CommandResponseType.Warning);
        }
    }
}
