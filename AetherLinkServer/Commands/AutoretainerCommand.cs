using System.Threading.Tasks;
using AetherLinkServer.Attributes;
using AetherLinkServer.Handlers;
using AetherLinkServer.Models;
using AetherLinkServer.Services;
using AetherLinkServer.Utility;

namespace AetherLinkServer.Commands;

public class AutoretainerCommand : CommandBase
{
    private readonly AutoRetainerHandler _autoRetainerHandler;
    
    [Command("autoretainer", "Autoretainer management")]
    public AutoretainerCommand(AutoRetainerHandler aRHandler)
    {
        _autoRetainerHandler = aRHandler;
    }
    public async Task Execute(string args)
    {
        if (args == string.Empty)
        {
            await SendCommandResponse("No arguments provided for command Autoretainer. See 'autoretainer help' for more information.",
                                      CommandResponseType.Warning);
        }

        switch (args.ToLowerInvariant())
        {
            case "autobell enable":
                _autoRetainerHandler.Invoke();
                await SendCommandResponse("Autoretainer autobell enabled", 
                                          CommandResponseType.Success);
                break;
        }
    }
}
