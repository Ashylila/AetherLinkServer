using System.Threading.Tasks;
using AetherLinkServer.Attributes;
using AetherLinkServer.Handlers;
using AetherLinkServer.Models;

namespace AetherLinkServer.Commands;

public class AutogatherCommand: CommandBase
{
    private readonly AutogatherHandler _autogatherHandler;
    public AutogatherCommand(AutogatherHandler autogatherHandler)
    {
        _autogatherHandler = autogatherHandler;
    }

    [Command("autogather", "Manage autogathering")]
    public async Task Execute(string args)
    {
        switch (args.ToLowerInvariant())
        {
            case "":
                await SendCommandResponse("Command cannot be used without arguments. See 'autogather help'",
                                          CommandResponseType.Error);
                break;
            case "enable":
                var result = _autogatherHandler.Invoke();
                if(result) await SendCommandResponse("Autogather successfully enabled", CommandResponseType.Success);
                else await SendCommandResponse("Autogather failed to enabled", CommandResponseType.Error);
                break;
            case "disable":
                _autogatherHandler.Disable();
                await SendCommandResponse("Autogather successfully disabled", CommandResponseType.Success);
                break;
        }
    }
}
