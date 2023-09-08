using System.Net.WebSockets;
using System.Reactive.Linq;

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
    public bool IsConnected { get; private set; } = false;

    public MaxDataClient(string clientId = "", int reconnectTimeout = 30, int keepAliveInterval = 30)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://max-api.maicoin.com")
        };

        var websocketFactory = new Func<ClientWebSocket>(() =>
        {
            var client = new ClientWebSocket
            {
                Options = { KeepAliveInterval = TimeSpan.FromSeconds(keepAliveInterval) }
            };
            return client;
        });

        _websocketClient = new WebsocketClient(new Uri("wss://max-stream.maicoin.com/ws"), websocketFactory)
        {
            Name = string.IsNullOrEmpty(clientId) ? typeof(MaxDataClient).Assembly.GetName().Name : clientId,
            IsReconnectionEnabled = true,
            ReconnectTimeout = TimeSpan.FromSeconds(reconnectTimeout),
            ErrorReconnectTimeout = TimeSpan.FromSeconds(reconnectTimeout),
        };

        Id = clientId;
    }

    public async Task EstablishConnectionAsync()
    {
        if (IsConnected)
            return;

        DateTimeOffset serverTime = await GetServerTimeAsync();
        // TODO: If cant't get server time.        
        SubscribeWebsocketEvents();
        // TODO: Logging message. 
        Console.WriteLine($"{Id} Connecting to server...");
        await _websocketClient.StartOrFail();
    }

    public async Task CloseConnectionAsync()
    {
        if (!IsConnected)
            return;

        Console.WriteLine($"{Id} Disconnecting from server...");
        await _websocketClient.StopOrFail(WebSocketCloseStatus.NormalClosure, WebSocketCloseStatus.NormalClosure.ToString());

        // TODO: Unsubscribe events.
    }

    public void Ping()
    {
        if (!IsConnected) return;

        var request = new
        {
            Id,
            Action = "ping"
        };
        _websocketClient.Send(JsonConvert.SerializeObject(request));
    }

    public void SubscribeMarketStatus()
    {
        if (!IsConnected) return;

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
        if (!IsConnected) return;

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
        if (!IsConnected) return;

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
        if (!IsConnected) return;

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

    public async Task<DateTimeOffset> GetServerTimeAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/v2/timestamp");
        var response = await _httpClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(content));
        return dateTime;
    }

    public async Task<IReadOnlyCollection<MarketResponse>> GetMarketsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/v2/markets");
        var response = await _httpClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        var markets = JsonConvert.DeserializeObject<IEnumerable<MarketResponse>>(content)
            ?? Array.Empty<MarketResponse>();
        return markets.ToArray().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<CurrencyResponse>> GetCurrenciesAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/v2/currencies");
        var response = await _httpClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        var markets = JsonConvert.DeserializeObject<IEnumerable<CurrencyResponse>>(content)
            ?? Array.Empty<CurrencyResponse>();
        return markets.ToArray().AsReadOnly();
    }

    private void SubscribeWebsocketEvents()
    {
        _websocketClient.ReconnectionHappened.Subscribe(info =>
        {
            switch (info.Type)
            {
                case ReconnectionType.Initial:
                    IsConnected = true;
                    break;

                case ReconnectionType.NoMessageReceived:
                    Console.WriteLine($"Reconnection because no message received");
                    break;

                case ReconnectionType.ByUser:
                    Console.WriteLine($"Reconnection by user");
                    break;

                case ReconnectionType.ByServer:
                    Console.WriteLine($"Reconnection by server");
                    break;

                case ReconnectionType.Error:
                    Console.WriteLine($"Reconnection because error");
                    break;

                case ReconnectionType.Lost:
                    Console.WriteLine($"Reconnection because connection lost.");
                    break;

                default:
                    break;
            }
            Connected?.Invoke(this, new EventArgs());
        });

        _websocketClient.DisconnectionHappened.Subscribe(info =>
        {
            switch (info.Type)
            {
                case DisconnectionType.Exit:
                    Console.WriteLine($"Disconnect by disposed.");
                    break;
                case DisconnectionType.Lost:
                    Console.WriteLine($"Disconnect => connection lost.");
                    break;
                case DisconnectionType.NoMessageReceived:
                    Console.WriteLine($"Disconnect no message received");
                    break;
                case DisconnectionType.Error:
                    Console.WriteLine($"Disconnect by Error ");
                    break;
                case DisconnectionType.ByUser:
                    Console.WriteLine($"Disconnect by user ");
                    break;
                case DisconnectionType.ByServer:
                    Console.WriteLine($"Disconnect by SERVER");
                    break;
                default:
                    break;
            }
        });

        // TODO: Subscribe market status snapshot.
        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "market_status" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "snapshot")
            .Subscribe(message => Console.WriteLine($"MarketStatusSnapshot => {message.Text}"));

        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "market_status" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "update")
            .Subscribe(message => Console.WriteLine($"MarketStatus update => {message.Text}"));

        // subscribe trade message
        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "trade" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "snapshot")
            .Subscribe(message => Console.WriteLine($"Trade snapshot => {message.Text}"));

        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "trade" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "update")
            .Subscribe(HandleTradeUpdateMessage);

        // TODO: Subscribe k line
        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "kline" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "snapshot")
            .Subscribe(message => Console.WriteLine($"kline snapshot => {message.Text}"));

        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "kline" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "update")
            .Subscribe(message => Console.WriteLine($"kline update => {message.Text}"));

        // TODO: Subscribe orderbook
        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "book" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "snapshot")
            .Subscribe(message => Console.WriteLine($"book snapshot => {message.Text}"));

        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "book" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "update")
            .Subscribe(message => Console.WriteLine($"book update => {message.Text}"));

        // TODO: Subscribe ticker
        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "ticker" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "snapshot")
            .Subscribe(message => Console.WriteLine($"ticker snapshot => {message.Text}"));

        _websocketClient.MessageReceived
            .Where(message => !string.IsNullOrEmpty(message.Text) &&
                JObject.Parse(message.Text).SelectToken("c")?.Value<string>() == "ticker" &&
                JObject.Parse(message.Text).SelectToken("e")?.Value<string>() == "update")
            .Subscribe(message => Console.WriteLine($"ticker update => {message.Text}"));

    }

    private string GenerateSignature(long nonce)
    {
        // using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        // var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{nonce}"));
        // return BitConverter.ToString(signatureBytes).Replace("-", "").ToLowerInvariant();
        throw new NotImplementedException();
    }

    private void HandleTradeUpdateMessage(ResponseMessage message)
    {
        if (string.IsNullOrEmpty(message.Text))
            return;

        var json = JObject.Parse(message.Text);
        var tradeData = json.SelectToken("t")?.Value<JArray>();
        if (tradeData is null) return;

        var marketId = json.SelectToken("M")?.Value<string>() ?? string.Empty;
        var eventTimestamp = json.SelectToken("T")?.Value<long>() ?? 0;
        foreach (var data in tradeData)
        {
            var timestamp = data.SelectToken("T")?.Value<long>() ?? 0;
            var price = data.SelectToken("p")?.Value<decimal>() ?? 0;
            var volume = data.SelectToken("v")?.Value<decimal>() ?? 0;
            var trend = data.SelectToken("tr")?.Value<string>() ?? string.Empty;

            TradeUpdate?.Invoke(this, new TradeUpdatedEvent
            {
                DateTime = DateTimeOffset.FromUnixTimeMilliseconds(eventTimestamp),
                MarketId = marketId,
                TradedTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp),
                TradedPrice = price,
                TradedVolume = volume,
                Trend = trend
            });
        }
    }
}
