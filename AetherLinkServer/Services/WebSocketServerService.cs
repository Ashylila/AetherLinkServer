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

    private readonly CancellationTokenSource _cts = new();
    private HttpListener _listener;
    public WebSocket _webSocket;
    private readonly Plugin plugin;

    public WebSocketServer(Plugin plugin)
    {
        this.plugin = plugin;
        StartServer();
    }

    private IPluginLog Logger => Svc.Log;

    public void Dispose()
    {
        _cts.Cancel(); // Cancel tasks
        _ = Task.Run(async () =>
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Plugin shutting down",
                    CancellationToken.None
                );
            }
        }).ContinueWith(t =>
        {
            _listener?.Stop();
            _listener?.Close();
            _cts.Dispose();
        });
    }

    public event CommandReceivedHandler OnCommandReceived;

    private async void StartServer()
    {
        try
        {
            //var ip = await PortForwarding.GetPublicIpAddress();
            //var result = await PortForwarding.EnableUpnpPortForwarding(plugin.Configuration.Port);
            //if (!result) return;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{plugin.Configuration.Port}/");
            _listener.Start();
            Logger.Debug($"WebSocket Server started on localhost:{plugin.Configuration.Port}");

            while (!_cts.Token.IsCancellationRequested)
                try
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
                catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                {
                    Logger.Debug("HttpListener was stopped.");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unexpected error in StartServer: {ex}");
                }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to start WebSocket server: {ex}");
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
                    var message = JsonSerializer.Deserialize<WebSocketMessage<object>>(json);
                    if (message != null)
                    {
                        Logger.Debug($"Received WebSocket message: {message.Type.ToString()} - {message.Data}");
                        ProcessMessage(message);
                    }
                }
                catch (Exception ex)
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

    public async Task SendMessage<T>(WebSocketMessage<T> webSocketMessage)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            var messageJson = JsonSerializer.Serialize(webSocketMessage);
            Logger.Verbose($"Sending message: {messageJson}");
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true,
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
