
using System;
using Dalamud.Game.Text;
#nullable disable

namespace AetherLinkServer.Models;
public class ChatMessage
{
    public string Sender { get; set; }
    public string Message { get; set; }
    public XivChatType Type { get; set; }
    public DateTime Timestamp { get; set; }
}