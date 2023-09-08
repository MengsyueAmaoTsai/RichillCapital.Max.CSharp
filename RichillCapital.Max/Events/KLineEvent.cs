
using RichillCapital.Max.Serialization;

namespace RichillCapital.Max.Events;

public sealed record KLineEvent
{
    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset DateTime { get; init; }

    [JsonProperty("M")]
    public string MarketId { get; init; } = string.Empty;

    [JsonProperty("k")]
    public KLineMessage KLine { get; init; } = new();
}

public sealed record KLineMessage
{
    [JsonProperty("M")]
    public string MarketId { get; init; } = string.Empty;

    [JsonProperty("R")]
    public string Resolution { get; init; } = string.Empty;

    [JsonProperty("ST")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset OpenTime { get; init; }

    [JsonProperty("ET")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset CloseTime { get; init; }

    [JsonProperty("O")]
    public decimal Open { get; init; }

    [JsonProperty("H")]
    public decimal High { get; init; }

    [JsonProperty("L")]
    public decimal Low { get; init; }

    [JsonProperty("C")]
    public decimal Close { get; init; }

    [JsonProperty("v")]
    public decimal Volume { get; init; }

    [JsonProperty("ti")]
    public long LastTradeId { get; init; }

    [JsonProperty("x")]
    public bool Closed { get; init; }

}