



using RichillCapital.Max;
using RichillCapital.Max.Events;

MaxDataClient client = new();

client.TradeUpdate += TradeUpdate;

void TradeUpdate(object? sender, TradeUpdatedEvent e) => Console.WriteLine($"{e.DateTime} {e.MarketId} {e.Price} {e.Volume}");

await client.EstablishConnectionAsync();

var markets = await client.GetMarketsAsync();
foreach (var market in markets)
{
    client.SubscribeTrade(market.Id);
}

Console.ReadLine();