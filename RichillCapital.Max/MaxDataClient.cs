
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reflection;

using Newtonsoft.Json.Linq;

using RichillCapital.Max.Events;
using RichillCapital.Max.Models;

using Websocket.Client;

namespace RichillCapital.Max;

public sealed partial class MaxDataClient
{
    private readonly HttpClient _httpClient;
    private readonly WebsocketClient _websocketClient;

    public string Id { get; private set; }
    public bool IsConnected => _websocketClient.IsRunning;

    public event EventHandler<EventArgs>? Connected;
    public event EventHandler<EventArgs>? Disconnected;

    public event EventHandler<PongEvent>? Pong;
    public event EventHandler<ErrorEvent>? Error;
    public event EventHandler<SubscribedEvent>? Subscribed;
    public event EventHandler<UnsubscribedEvent>? Unsubscribed;

    public event EventHandler<MarketStatusEvent>? MarketStatusUpdated;
    public event EventHandler<MarketStatusEvent>? MarketStatusSnapshot;
    public event EventHandler<TradeEvent>? TradeUpdated;
    public event EventHandler<TradeEvent>? TradeSnapshot;
    public event EventHandler<TickerEvent>? TickerUpdated;
    public event EventHandler<TickerEvent>? TickerSnapshot;
    public event EventHandler<KLineEvent>? KLineUpdated;
    public event EventHandler<KLineEvent>? KLineSnapshot;
    public event EventHandler<OrderbookEvent>? OrderbookUpdated;
    public event EventHandler<OrderbookEvent>? OrderbookSnapshot;

    public MaxDataClient(
        string id = "",
        int keepAliveInterval = 15,
        int reconnectTimeout = 30,
        int errorReconnectTimeout = 30)
    {
        string? executingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        string? assemblyName = typeof(MaxDataClient).Assembly.GetName().Name;

        Id = string.IsNullOrEmpty(id) ?
            string.IsNullOrEmpty(executingAssemblyName) ?
            string.IsNullOrEmpty(assemblyName) ?
            "RichillCapital.Max" : assemblyName : executingAssemblyName : id;

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

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "pong")
            .Subscribe(OnPongMessage);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "error")
            .Subscribe(OnErrorMessage);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "subscribed")
            .Subscribe(OnSubscribed);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "unsubscribed")
            .Subscribe(OnUnsubscribed);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "snapshot" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "market_status")
            .Subscribe(OnMarketStatusSnapshot);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "update" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "market_status")
            .Subscribe(OnMarketStatusUpdate);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "snapshot" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "trade")
            .Subscribe(OnTradeSnapshot);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "update" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "trade")
            .Subscribe(OnTradeUpdate);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "snapshot" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "ticker")
            .Subscribe(OnTickerSnapshot);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "update" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "ticker")
            .Subscribe(OnTickerUpdate);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "snapshot" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "kline")
            .Subscribe(OnKLineSnapshot);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "update" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "kline")
            .Subscribe(OnKLineUpdate);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "snapshot" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "book")
            .Subscribe(OnOrderbookSnapshot);

        _websocketClient.MessageReceived
            .Where(message =>
                !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text)?.SelectToken("e")?.Value<string>() == "update" &&
                JObject.Parse(message.Text)?.SelectToken("c")?.Value<string>() == "book")
            .Subscribe(OnOrderbookUpdate);
    }

    public async Task EstablishConnectionAsync()
    {
        if (IsConnected)
            return;
        // Get server time to check rest server is alive.
        var serverTime = await GetServerTimeAsync();

        if (serverTime is null)
            return;

        await _websocketClient.Start();
    }

    public async Task CloseConnectionAsync()
    {
        if (!IsConnected)
            return;
        await _websocketClient.Stop(WebSocketCloseStatus.NormalClosure, "Called CloseConnectionAsync().");
    }

    public async Task<DateTimeOffset?> GetServerTimeAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v2/timestamp");
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return DateTimeOffset.FromUnixTimeSeconds(long.Parse(content));
    }

    public async Task<IReadOnlyCollection<MarketResponse>> GetMarketsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v2/markets");
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var markets = JsonConvert.DeserializeObject<IEnumerable<MarketResponse>>(content) ?? new List<MarketResponse>();
        return markets.ToList().AsReadOnly();
    }

    public void Ping()
    {
        var request = new
        {
            Action = "ping"
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void SubscribeMarketStatus()
    {
        var request = new
        {
            Id,
            Action = "sub",
            Subscriptions = new object[]
            {
                new { Channel = "market_status" }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void UnsubscribeMarketStatus()
    {
        var request = new
        {
            Id,
            Action = "unsub",
            Subscriptions = new object[]
            {
                new { Channel = "market_status" }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void SubscribeTrade(string marketId)
    {
        var request = new
        {
            Id,
            Action = "sub",
            Subscriptions = new object[]
            {
                new { Channel = "trade", Market = marketId }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void UnsubscribeTrade(string marketId)
    {
        var request = new
        {
            Id,
            Action = "unsub",
            Subscriptions = new object[]
            {
                new { Channel = "trade", Market = marketId }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void SubscribeTicker(string marketId)
    {
        var request = new
        {
            Id,
            Action = "sub",
            Subscriptions = new object[]
            {
                new { Channel = "ticker", Market = marketId }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void UnsubscribeTicker(string marketId)
    {
        var request = new
        {
            Id,
            Action = "unsub",
            Subscriptions = new object[]
            {
                new { Channel = "ticker", Market = marketId }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void SubscribeKLine(string marketId, string resolution = "1m")
    {
        var request = new
        {
            Id,
            Action = "sub",
            Subscriptions = new object[]
            {
                new { Channel = "kline", Market = marketId, Resolution = resolution }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void UnsubscribeKLine(string marketId, string resolution = "1m")
    {
        var request = new
        {
            Id,
            Action = "unsub",
            Subscriptions = new object[]
            {
                new { Channel = "kline", Market = marketId, Resolution = resolution }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void SubscribeOrderbook(string marketId, int depth = 10)
    {
        var request = new
        {
            Id,
            Action = "sub",
            Subscriptions = new object[]
            {
                new { Channel = "book", Market = marketId, Depth = depth }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void UnsubscribeOrderbook(string marketId, int depth = 10)
    {
        var request = new
        {
            Id,
            Action = "unsub",
            Subscriptions = new object[]
            {
                new { Channel = "book", Market = marketId, Depth = depth }
            },
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    private void OnReconnectingHappened(ReconnectionInfo info)
    {
        Console.WriteLine($"Reconnection happened, type: {info.Type}, url: {_websocketClient.Url}");
        Connected?.Invoke(this, new EventArgs());
    }

    private void OnDisconnectionHappened(DisconnectionInfo info)
    {
        Console.WriteLine($"Disconnection happened, type: {info.Type}");
        Disconnected?.Invoke(this, new EventArgs());
    }

    private void OnPongMessage(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;

        var @event = JsonConvert.DeserializeObject<PongEvent>(message.Text);

        if (@event is null) return;

        Pong?.Invoke(this, @event);
    }

    private void OnErrorMessage(ResponseMessage message)
    {
        Console.WriteLine($"Error => {message.Text}");
        if (string.IsNullOrEmpty(message.Text))
            return;

        var @event = JsonConvert.DeserializeObject<ErrorEvent>(message.Text);

        if (@event is null)
            return;

        Error?.Invoke(this, @event);
    }

    private void OnSubscribed(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text))
            return;

        var @event = JsonConvert.DeserializeObject<SubscribedEvent>(message.Text);

        if (@event is null)
            return;

        Subscribed?.Invoke(this, @event);
    }

    private void OnUnsubscribed(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text))
            return;

        var @event = JsonConvert.DeserializeObject<UnsubscribedEvent>(message.Text);

        if (@event is null)
            return;

        Unsubscribed?.Invoke(this, @event);
    }

    private void OnMarketStatusSnapshot(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;
        var @event = JsonConvert.DeserializeObject<MarketStatusEvent>(message.Text);
        if (@event is null) return;
        MarketStatusSnapshot?.Invoke(this, @event);
    }

    private void OnMarketStatusUpdate(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;
        var @event = JsonConvert.DeserializeObject<MarketStatusEvent>(message.Text);
        if (@event is null) return;
        MarketStatusUpdated?.Invoke(this, @event);
    }

    private void OnTradeUpdate(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;
        var @event = JsonConvert.DeserializeObject<TradeEvent>(message.Text);
        if (@event is null) return;
        TradeUpdated?.Invoke(this, @event);
    }

    private void OnTradeSnapshot(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;
        var @event = JsonConvert.DeserializeObject<TradeEvent>(message.Text);
        if (@event is null) return;
        TradeSnapshot?.Invoke(this, @event);
    }

    private void OnTickerUpdate(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;
        var @event = JsonConvert.DeserializeObject<TickerEvent>(message.Text);
        if (@event is null) return;
        TickerUpdated?.Invoke(this, @event);
    }

    private void OnTickerSnapshot(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;
        var @event = JsonConvert.DeserializeObject<TickerEvent>(message.Text);
        if (@event is null) return;
        TickerSnapshot?.Invoke(this, @event);
    }

    private void OnKLineUpdate(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;
        var @event = JsonConvert.DeserializeObject<KLineEvent>(message.Text);
        if (@event is null) return;
        KLineUpdated?.Invoke(this, @event);
    }

    private void OnKLineSnapshot(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;
        var @event = JsonConvert.DeserializeObject<KLineEvent>(message.Text);
        if (@event is null) return;
        KLineSnapshot?.Invoke(this, @event);
    }

    private void OnOrderbookSnapshot(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text)) return;
        var @event = JsonConvert.DeserializeObject<OrderbookEvent>(message.Text);
        if (@event is null) return;
        OrderbookSnapshot?.Invoke(this, @event);
    }

    private void OnOrderbookUpdate(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        var @event = JsonConvert.DeserializeObject<OrderbookEvent>(message.Text);

        if (@event is null)
        {
            return;
        }

        OrderbookUpdated?.Invoke(this, @event);
    }
}