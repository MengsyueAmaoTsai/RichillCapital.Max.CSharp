
using RichillCapital.Max.Serialization;

namespace RichillCapital.Max.Events;

public sealed record SubscribedEvent
{
    [JsonProperty("T")]
    [JsonConverter(typeof(MillisecondTimestampConverter))]
    public DateTimeOffset DateTime { get; init; }

    [JsonProperty("i")]
    public string ClientId { get; init; } = string.Empty;

    [JsonProperty("s")]
    public string[] Subscriptions { get; init; } = Array.Empty<string>();
}