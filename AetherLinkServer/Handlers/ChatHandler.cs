using System;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Models;
using AetherLinkServer.Utility;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;

namespace AetherLinkServer.Handlers;

public class ChatHandler : IDisposable
{
    private Plugin plugin;

    public ChatHandler(Plugin plugin)
    {
        this.plugin = plugin;
        _chatGui.ChatMessage += OnChatMessageReceived;
    }

    private IChatGui _chatGui => Svc.Chat;
    private IPluginLog Logger => Svc.Log;

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
        Logger.Debug($"Chat message received: {chatMessage.Sender}: {chatMessage.Message}");
        _ = Task.Run(() => CommandHelper.SendCommand(chatMessage, WebSocketActionType.ChatMessage));
    }
}
