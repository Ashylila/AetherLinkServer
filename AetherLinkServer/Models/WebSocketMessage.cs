namespace AetherLinkServer.Models;
#nullable disable
public class WebSocketMessage
{
    public WebSocketActionType Type { get; set; }
    public string Data { get; set; }
}