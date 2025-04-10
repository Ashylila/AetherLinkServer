using System;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Models;
using AetherLinkServer.Services;
using AetherLinkServer.Utility;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;

namespace AetherLinkServer.Handlers;

public class ChatHandler : IDisposable
{
    private Plugin plugin;
    private readonly WebSocketServer _server;
    private readonly IPluginLog _logger;
    private readonly IChatGui _chatGui;

    public ChatHandler(Plugin plugin, WebSocketServer server, IChatGui chatGui, IPluginLog logger)
    {
        _chatGui = chatGui;
        _logger = logger;
        _server = server;
        this.plugin = plugin;
        _chatGui.ChatMessage += OnChatMessageReceived;
    }
    

    public void Dispose()
    {
        _chatGui.ChatMessage -= OnChatMessageReceived;
    }

    private void OnChatMessageReceived(
        XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if ((int)type >= 2000) return;
        var chatMessage = new ChatMessage
        {
            Type = type,
            Sender = sender.TextValue,
            Timestamp = DateTime.Now,
            Message = message.TextValue
        };
        _logger.Debug($"Chat message received: {chatMessage.Sender}: {chatMessage.Message}");
        _ = Task.Run(() => _server.SendCommand(chatMessage, WebSocketActionType.ChatMessage));
    }
}
