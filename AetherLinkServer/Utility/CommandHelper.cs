using System;
using System.Text.Json;
using System.Threading.Tasks;
using AetherLinkServer.Models;
using ECommons.DalamudServices;
using FFXIVClientStructs;

namespace AetherLinkServer.Utility;

public static class CommandHelper
{
    private static WebSocketMessage<T> CreateCommand<T>(T command, WebSocketActionType type)
    {
        var websocketcommand = new WebSocketMessage<T>
        {
            Type = type,
            Data = command
        };
        return websocketcommand;
    }

    public static async Task SendCommand<T>(T command, WebSocketActionType type)
    {
        var websocketcommand = CreateCommand(command, type);
        var json = JsonSerializer.Serialize(websocketcommand);
        
        await Plugin.server.SendMessage(websocketcommand);
    }

    public static async Task SendCommandResponse(string message, CommandResponseType type)
    {
        var commandResponse = new CommandResponse
        {
            Message = message,
            Type = type
        };
        var command = CreateCommand(commandResponse, WebSocketActionType.CommandResponse);
        await Plugin.server.SendMessage(command);
    }
}
