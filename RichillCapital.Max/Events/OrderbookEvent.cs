
using RichillCapital.Max.Serialization;

namespace RichillCapital.Max.Events;

public sealed record OrderbookEvent
{
    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset DateTime { get; init; }

    [JsonProperty("M")]
    public string MarketId { get; init; } = string.Empty;
}