
namespace RichillCapital.Max.Events;

public sealed class TradeUpdatedEvent : EventArgs
{
    public string MarketId { get; init; } = string.Empty;
    public DateTimeOffset DateTime { get; init; }
    public decimal Price { get; init; }
    public decimal Volume { get; init; }
    public string Trend { get; init; } = string.Empty;
}