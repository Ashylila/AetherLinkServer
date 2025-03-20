using AetherLinkServer.Services;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using AetherLinkServer.Models;
using System;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
using AetherLinkServer;
public class ChatHandler : IDisposable
{

    private IChatGui _chatGui => Svc.Chat;
    private Plugin plugin;
    public ChatHandler(Plugin plugin)
    {
        this.plugin = plugin;
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
        var webMessage = new WebSocketMessage<ChatMessage>
        {
            Type = WebSocketActionType.ChatMessage,
            Data = chatMessage
        };
        _ = Task.Run(() => Plugin.server.SendMessage(webMessage));
    }
    public void Dispose()
    {
        _chatGui.ChatMessage -= OnChatMessageReceived;
    }
}