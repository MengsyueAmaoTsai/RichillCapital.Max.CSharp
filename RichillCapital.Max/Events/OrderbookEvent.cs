using RichillCapital.Max.Serialization;

namespace RichillCapital.Max.Events;

[JsonConverter(typeof(OrderBookEventConverter))]
public sealed record OrderbookEvent
{
    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset DateTime { get; init; }

    [JsonProperty("M")]
    public string MarketId { get; init; } = string.Empty;

    [JsonProperty("a")]
    public OrderbookEntry[] Asks { get; init; } = Array.Empty<OrderbookEntry>();

    [JsonProperty("b")]
    public OrderbookEntry[] Bids { get; init; } = Array.Empty<OrderbookEntry>();
}

public sealed record OrderbookEntry
{
    public decimal Price { get; init; }
    public decimal Volume { get; init; }
}
