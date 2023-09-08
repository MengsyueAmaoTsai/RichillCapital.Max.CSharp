

using System.Net.WebSockets;
using System.Reflection;

using Microsoft.Extensions.Logging;

using RichillCapital.Max.Models;

using Websocket.Client;

namespace RichillCapital.Max;

public sealed class MaxDataClient
{
    public string Id { get; private set; }
    public bool IsConnected { get; private set; } = false;

    private readonly HttpClient _httpClient;
    private readonly WebsocketClient _websocketClient;

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
}
