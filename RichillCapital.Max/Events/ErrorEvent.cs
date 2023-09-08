
using RichillCapital.Max.Serialization;

namespace RichillCapital.Max.Events;

public sealed record ErrorEvent
{
    [JsonProperty("E")]
    public string[] Errors { get; init; } = Array.Empty<string>();

    [JsonProperty("i")]
    public string ClientId { get; init; } = string.Empty;

    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset DateTime { get; init; }
}