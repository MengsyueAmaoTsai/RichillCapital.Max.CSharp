
using RichillCapital.Max.Serialization;

namespace RichillCapital.Max.Models;

public sealed record PongEvent
{
    [JsonProperty("i")]
    public string ClientId { get; init; } = string.Empty;

    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset DateTime { get; init; }
}