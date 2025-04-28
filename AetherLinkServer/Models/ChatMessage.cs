#nullable disable

using System;
using Dalamud.Game.Text;

namespace AetherLinkServer.Models;

public class ChatMessage
{
    public string Sender { get; set; }
    public string Message { get; set; }
    public XivChatType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
}
