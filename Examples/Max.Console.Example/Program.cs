



using RichillCapital.Max;
using RichillCapital.Max.Events;

MaxDataClient client = new();

client.TradeUpdate += TradeUpdate;


await client.EstablishConnectionAsync();

var markets = await client.GetMarketsAsync();
foreach (var market in markets)
{
    client.SubscribeTrade(market.Id);
}

Console.ReadLine();

static void TradeUpdate(object? sender, TradeUpdatedEvent e) => Console.WriteLine($"{e.TradedTime} {e.MarketId} {e.TradedPrice} {e.TradedVolume}");
