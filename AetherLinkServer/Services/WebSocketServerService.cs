using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AetherLinkServer.DalamudServices;
using AetherLinkServer.Models;
using Dalamud.Plugin.Services;

namespace AetherLinkServer.Services;
#nullable disable
public class WebSocketServer : IDisposable
{
    public delegate Task CommandReceivedHandler(string command, string args);
    
    private readonly Plugin plugin;
    private ClientWebSocket client;
    private CancellationTokenSource cts;
    public WebSocketServer(Plugin plugin)
    {
        this.plugin = plugin;
        StartListener();
    }

    private IPluginLog Logger => Svc.Log;

    public void Dispose()
    {
        cts?.Cancel();
        try
        {
            if (client?.State == WebSocketState.Open)
            {
                client.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Plugin unloading",
                    CancellationToken.None).Wait(1000); // Timeout to prevent hangs
            }
        }
        finally
        {
            client?.Dispose();
            client?.Dispose();
        }
    }
    public event CommandReceivedHandler OnCommandReceived;

    private async void StartListener()
    {
        client = new ClientWebSocket();
        try
        {
            cts = new CancellationTokenSource();
            await client.ConnectAsync(new Uri("ws://65.38.98.16:5000/ws"), cts.Token);
            Logger.Debug("WebSocket client connected");
            _ = ReceiveMessages();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to connect to WebSocket server: {ex}");
        }
    }


    private async Task ReceiveMessages()
    {
        var buffer = new byte[1024];

        while (client?.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            try
            {
                var command = JsonSerializer.Deserialize<WebSocketMessage<object>>(json);
                
                ProcessMessage(command);
                                   
            }catch(Exception ex)
            {
                Logger.Error($"Failed to deserialize message: {ex}");   
                continue;
            }

        }
    }

    public async Task SendMessage<T>(WebSocketMessage<T> webSocketMessage)
    {
        if (client?.State == WebSocketState.Open)
        {
            var messageJson = JsonSerializer.Serialize(webSocketMessage);
            Logger.Verbose($"Sending message: {messageJson}");
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            await client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true,
                                       CancellationToken.None);
        }
    }

    private void ProcessMessage(WebSocketMessage<object> message)
    {
        if (message.Data is JsonElement data && data.ValueKind == JsonValueKind.String)
        {
            switch (message.Type)
            {
                case WebSocketActionType.Command:
                    ProcessCommand(data.GetString());
                    break;
                case WebSocketActionType.SendChatMessage:
                    SendChatMessage(data.GetString());
                    break;
                default:
                    Logger.Error($"Unknown message type: {message.Type}");
                    break;
            }
        }
    }

    private void ProcessCommand(string command)
    {
        var parts = command.Split(' ');
        var cmd = parts[0];
        var args = string.Join(' ', parts[1..]);
        OnCommandReceived?.Invoke(cmd, args);
    }

    private void SendChatMessage(string message)
    {
        throw new NotImplementedException();
    }
}
