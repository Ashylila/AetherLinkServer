using System;
using System.Text.Json;
using System.Threading.Tasks;
using AetherLinkServer.Models;
using AetherLinkServer.Services;
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

    public static async Task SendCommand<T>(this WebSocketServer server,T command, WebSocketActionType type)
    {
        var websocketcommand = CreateCommand(command, type);
        var json = JsonSerializer.Serialize(websocketcommand);
        
        await server.SendMessage(websocketcommand);
    }

    public static async Task SendCommandResponse(this WebSocketServer server,string message, CommandResponseType type)
    {
        var commandResponse = new CommandResponse
        {
            Message = message,
            Type = type
        };
        var command = CreateCommand(commandResponse, WebSocketActionType.CommandResponse);
        await server.SendMessage(command);
    }
}
