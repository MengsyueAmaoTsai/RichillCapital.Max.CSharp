using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reflection;

using Newtonsoft.Json.Linq;

using RichillCapital.Max.Events;
using RichillCapital.Max.Models;

using Websocket.Client;

namespace RichillCapital.Max;

public sealed class MaxDataClient
{
    private readonly HttpClient _httpClient;
    private readonly WebsocketClient _websocketClient;

    public string Id { get; private set; }
    public bool IsConnected { get; private set; } = false;

    public event EventHandler? Connected;
    public event EventHandler? Disconnect;
    public event EventHandler? MarketStatusUpdate;
    public event EventHandler? MarketStatusSnapshot;
    public event EventHandler<TradeUpdatedEvent>? TradeUpdate;
    public event EventHandler? TradeSnapshot;
    public event EventHandler? TickerUpdate;
    public event EventHandler? TickerSnapshot;
    public event EventHandler? KLineUpdate;
    public event EventHandler? KLineSnapshot;
    public event EventHandler? OrderbookUpdate;
    public event EventHandler? OrderbookSnapshot;

    public MaxDataClient(string clientId = "", int reconnectTimeout = 30)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://max-api.maicoin.com")
        };
        _websocketClient = new WebsocketClient(new Uri("wss://max-stream.maicoin.com/ws"))
        {
            Name = string.IsNullOrEmpty(clientId) ? clientId : Assembly.GetExecutingAssembly().GetName().Name,
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
        SubscribeWebsocketEvents();
        Console.WriteLine($"{Id} Connecting to server...");

        await _websocketClient.StartOrFail();
    }

    public async Task CloseConnectionAsync()
    {
        if (!IsConnected)
            return;

        Console.WriteLine($"{Id} Disconnecting from server...");
        await _websocketClient.StopOrFail(WebSocketCloseStatus.NormalClosure, WebSocketCloseStatus.NormalClosure.ToString());
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

        // Subscribe market status snapshot.
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
            .Subscribe(message =>
            {
                TradeUpdate?.Invoke(this, new TradeUpdatedEvent
                {
                });
            });

    }

    private string GenerateSignature(long nonce)
    {
        // using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        // var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{nonce}"));
        // return BitConverter.ToString(signatureBytes).Replace("-", "").ToLowerInvariant();
        throw new NotImplementedException();
    }
}
