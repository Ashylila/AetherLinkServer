
using AetherLinkServer.Models;
using System.Text;

using System.Text.Json;
namespace AetherLinkServer.Utility;
public static class CommandHelper
{
    private static WebSocketMessage<T> CreateCommand<T> (T command, WebSocketActionType type)
    {
        var websocketcommand = new WebSocketMessage<T>
        {
            Type = type,
            Data = command
        };
        return websocketcommand;
    }
    public async static void SendCommand<T>(T command, WebSocketActionType type)
    {
        var websocketcommand = CreateCommand(command, type);
        var json = JsonSerializer.Serialize(websocketcommand);
        await Plugin.server.SendMessage(websocketcommand);
    }
}