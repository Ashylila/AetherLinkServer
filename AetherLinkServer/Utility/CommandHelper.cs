
using AetherLinkServer.Models;
using System;
using System.Text;

using System.Text.Json;
using System.Threading.Tasks;
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
    public async static Task SendCommand<T>(T command, WebSocketActionType type)
    {
        var websocketcommand = CreateCommand(command, type);
        var json = JsonSerializer.Serialize(websocketcommand);
        await Plugin.server.SendMessage(websocketcommand);
    }
    public async static Task SendCommandResponse(string message, CommandResponseType type)
    {
        var commandResponse = new CommandResponse
        {
            Message = message,
            Type = type,
            TimeStamp = DateTime.Now
        };
        var websocketcommand = CreateCommand(commandResponse, WebSocketActionType.CommandResponse);
        await Plugin.server.SendMessage(websocketcommand);
    }
}