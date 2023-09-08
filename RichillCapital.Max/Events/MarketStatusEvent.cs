using RichillCapital.Max.Models;
using RichillCapital.Max.Serialization;

namespace RichillCapital.Max.Events;

public sealed record MarketStatusEvent
{
    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset DateTime { get; init; }

    [JsonProperty("ms")]
    public MarketMessage[] Markets { get; init; } = Array.Empty<MarketMessage>();
}