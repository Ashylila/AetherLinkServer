namespace AetherLinkServer.Models;

public enum WebSocketActionType
{
    ChatMessageReceived,
    SendChatMessage,
    Command,
    InvalidCommandUsage,
    CommandResponse,
    ChatMessage
}
