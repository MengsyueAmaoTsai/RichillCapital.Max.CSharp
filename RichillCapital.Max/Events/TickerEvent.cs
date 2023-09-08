
using RichillCapital.Max.Serialization;

namespace RichillCapital.Max.Events;

public sealed record TickerEvent
{
    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset DateTime { get; init; }

    [JsonProperty("M")]
    public string MarketId { get; init; } = string.Empty;

    [JsonProperty("tk")]
    public TickerMessage Ticker { get; init; } = new();
}

public sealed record TickerMessage
{
    [JsonProperty("M")]
    public string MarketId { get; init; } = string.Empty;

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

    [JsonProperty("V")]
    public decimal VolumeInBtc { get; init; }
}