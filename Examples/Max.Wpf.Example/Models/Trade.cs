using System;

namespace Max.Wpf.Example.Models;

public sealed record Trade
{
    public string Symbol { get; init; } = string.Empty;
    public DateTimeOffset Time { get; init; }
    public decimal Price { get; init; }
    public decimal Volume { get; init; }
}
