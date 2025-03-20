using AetherLinkServer.Services;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using AetherLinkServer.Models;
using System;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
using AetherLinkServer;
using AetherLinkServer.Utility;
using WebSocketSharp;
public class ChatHandler : IDisposable
{

    private IChatGui _chatGui => Svc.Chat;
    private IPluginLog Logger => Svc.Log;
    private Plugin plugin;
    public ChatHandler(Plugin plugin)
    {
        this.plugin = plugin;
        _chatGui.ChatMessage += OnChatMessageReceived;
    }

    private void OnChatMessageReceived(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if((int)type >= 2000) return;
        var chatMessage = new ChatMessage
        {
            Type = type,
            Sender = sender.TextValue,
            Timestamp = DateTime.Now,
            Message = message.TextValue
        };
        Logger.Debug($"Chat message received: {chatMessage.Sender}: {chatMessage.Message}");
        _ = Task.Run(() => CommandHelper.SendCommand(chatMessage, WebSocketActionType.ChatMessage));
    }
    public void Dispose()
    {
        _chatGui.ChatMessage -= OnChatMessageReceived;
    }
}