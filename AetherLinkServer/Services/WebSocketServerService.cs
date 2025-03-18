using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using AetherLinkServer.Models;
using Dalamud.Plugin.Services;
using AetherLinkServer.DalamudServices;
using System.Text.Json;

namespace AetherLinkServer.Services;

public class WebSocketServer : IDisposable
{
    private Plugin plugin;
    private IPluginLog Logger => Svc.Log;
    private HttpListener _listener;
    private WebSocket _webSocket;
    private CancellationTokenSource _cts = new CancellationTokenSource();

    public delegate void CommandReceivedHandler(string command, string args);
    public event CommandReceivedHandler OnCommandReceived;

    public WebSocketServer(Plugin plugin)
    {
        this.plugin = plugin;
        StartServer();
    }

    private async void StartServer()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{plugin.Configuration.Port}/"); // Placeholder URL
        _listener.Start();
        Logger.Debug($"WebSocket Server started on localhost:{plugin.Configuration.Port}");

        while (!_cts.Token.IsCancellationRequested)
        {
            var context = await _listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var wsContext = await context.AcceptWebSocketAsync(null);
                _webSocket = wsContext.WebSocket;
                Logger.Debug("WebSocket connection established");
                _ = Task.Run(() => ListenForCommands());
            }
        }
    }

    private async Task ListenForCommands()
    {
        var buffer = new byte[1024];
        try
        {
            while (_webSocket?.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Logger.Debug("WebSocket connection closed");
                    break;
                }
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    var message = JsonSerializer.Deserialize<WebSocketMessage>(json);
                    if(message != null)
                    {
                        ProcessMessage(message);
                    }
                }catch(Exception ex)
                {
                    Logger.Error($"Error deserializing WebSocket message: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Error("WebSocket listener task cancelled.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in WebSocket listener: {ex.Message}");
        }
    }

    public async Task SendMessage(ChatMessage chatMessage)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            var messageJson = System.Text.Json.JsonSerializer.Serialize(chatMessage);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private void ProcessMessage(WebSocketMessage message)
    {
        switch(message.Type)
        {
            case WebSocketActionType.Command:
                ProcessCommand(message.Data);
                break;
            case WebSocketActionType.SendChatMessage:
                SendChatMessage(message.Data);
                break;
            default:
                Logger.Error($"Unknown message type: {message.Type}");
                break;
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
    public void Dispose()
    {
        _cts.Cancel();
        _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Plugin shutting down", CancellationToken.None);
        _listener?.Stop();
    }
}
