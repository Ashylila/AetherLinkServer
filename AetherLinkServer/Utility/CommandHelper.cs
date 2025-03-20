
using AetherLinkServer.Models;
using System.Text;

using System.Text.Json;
namespace AetherLinkServer.Utility;
public static class CommandHelper
{
    public static WebSocketMessage<T> createCommand<T> (T command, WebSocketActionType type)
    {
        var websocketcommand = new WebSocketMessage<T>
        {
            Type = type,
            Data = command
        };
        return websocketcommand;
    }
}