
namespace RichillCapital.Max.Models;

public sealed record MarketStatusSubscription
{
    public string Channel { get; init; } = "market_status";
}