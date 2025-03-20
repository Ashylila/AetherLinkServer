using AetherLinkServer.Services;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using AetherLinkServer.Models;
using System;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
public class ChatHandler : IDisposable
{
    private readonly WebSocketServer _webSocketServer;
    private IChatGui _chatGui => Svc.Chat;
    public ChatHandler(WebSocketServer webSocketServer)
    {
        _webSocketServer = webSocketServer;
        _chatGui.ChatMessage += OnChatMessageReceived;
    }

    private void OnChatMessageReceived(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if((int)type >= 8000) return;
        var chatMessage = new ChatMessage
        {
            Type = type,
            Sender = sender.TextValue,
            Timestamp = DateTime.Now,
            Message = message.TextValue
        };
        _ = Task.Run(() => _webSocketServer.SendMessage(chatMessage));
    }
    public void Dispose()
    {
        _chatGui.ChatMessage -= OnChatMessageReceived;
        _webSocketServer.Dispose();
    }
}