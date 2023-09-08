
using RichillCapital.Max.Serialization;

namespace RichillCapital.Max.Events;

public sealed record TradeEvent
{
    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset DateTime { get; init; }

    [JsonProperty("M")]
    public string MarketId { get; init; } = string.Empty;

    [JsonProperty("t")]
    public TradeMessage[] Trades { get; init; } = Array.Empty<TradeMessage>();
}

public sealed record TradeMessage
{
    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset Time { get; init; }

    [JsonProperty("p")]
    public decimal Price { get; init; }

    [JsonProperty("v")]
    public decimal Volume { get; init; }

    [JsonProperty("tr")]
    public string Trend { get; init; } = string.Empty;
}