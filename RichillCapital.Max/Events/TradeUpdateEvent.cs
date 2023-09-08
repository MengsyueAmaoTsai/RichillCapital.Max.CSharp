
namespace RichillCapital.Max.Events;

public sealed class TradeUpdatedEvent : EventArgs
{
    public DateTimeOffset DateTime { get; init; }
    public string MarketId { get; init; } = string.Empty;
    public DateTimeOffset TradedTime { get; init; }
    public decimal TradedPrice { get; init; }
    public decimal TradedVolume { get; init; }
    public string Trend { get; init; } = string.Empty;
}