
using System.Net.WebSockets;
using System.Reflection;

using Websocket.Client;

namespace RichillCapital.Max;

public sealed partial class MaxDataClient
{
    private readonly HttpClient _httpClient;
    private readonly WebsocketClient _websocketClient;

    public string Id { get; private set; }
    public bool IsConnected => _websocketClient.IsRunning;

    public MaxDataClient(
        string id,
        int keepAliveInterval = 30,
        int reconnectTimeout = 30,
        int errorReconnectTimeout = 30)
    {
        Id = id;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ServerInfo.RestUrl)
        };

        // Create websocket client.
        var websocketFactory = new Func<ClientWebSocket>(() =>
        {
            var client = new ClientWebSocket
            {
                Options = { KeepAliveInterval = TimeSpan.FromSeconds(keepAliveInterval) }
            };
            return client;
        });
        _websocketClient = new WebsocketClient(new Uri(ServerInfo.WebsocketUrl), websocketFactory)
        {
            Name = id,
            ReconnectTimeout = TimeSpan.FromSeconds(reconnectTimeout),
            ErrorReconnectTimeout = TimeSpan.FromSeconds(errorReconnectTimeout)
        };

        // Register websocket message handlers
        _websocketClient.ReconnectionHappened.Subscribe(OnReconnectingHappened);
        _websocketClient.DisconnectionHappened.Subscribe(OnDisconnectionHappened);
    }

    public async Task EstablishConnectionAsync()
    {
        if (IsConnected)
            return;
        // Get server time to check rest server is alive.

        // handle if not alive.

        await _websocketClient.Start();
    }

    public async Task CloseConnectionAsync()
    {
        if (!IsConnected)
            return;
        await _websocketClient.Stop(WebSocketCloseStatus.NormalClosure, "Called CloseConnectionAsync().");
    }

    private void OnDisconnectionHappened(DisconnectionInfo info)
    {
        throw new NotImplementedException();
    }

    private void OnReconnectingHappened(ReconnectionInfo info)
    {
        throw new NotImplementedException();
    }
}