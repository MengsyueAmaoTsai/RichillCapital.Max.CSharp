using RichillCapital.Max;
using RichillCapital.Max.Events;

MaxDataClient dataClient = new("Max.Console.Example");
dataClient.Pong += HandlePong;
dataClient.Error += HandleError;

dataClient.MarketStatusSnapshot += HandleMarketStatusSnapshot;
dataClient.MarketStatusUpdated += HandleMarketStatusUpdated;
dataClient.TradeSnapshot += HandleTradeSnapshot;
dataClient.TradeUpdated += HandleTradeUpdated;
dataClient.TickerSnapshot += HandleTickerSnapshot;
dataClient.TickerUpdated += HandleTickerUpdated;
dataClient.KLineSnapshot += HandleKLineSnapshot;
dataClient.KLineUpdated += HandleKLineUpdated;
dataClient.OrderbookSnapshot += HandleOrderbookSnapshot;
dataClient.OrderbookUpdated += HandleOrderbookUpdated;


Console.WriteLine("|====================================|");
Console.WriteLine("|    MaxDataClient Console Example   |");
Console.WriteLine("|====================================|");
Console.WriteLine();

Console.WriteLine("|====================================|");
Console.WriteLine("|              Starting              |");
Console.WriteLine("|====================================|");
Console.WriteLine();

var testSymbols = new string[] { "btctwd", "usdttwd", "ethtwd" };

await dataClient.EstablishConnectionAsync();
await dataClient.GetServerTimeAsync();
await dataClient.GetMarketsAsync();
await Task.Delay(2000);

dataClient.SubscribeMarketStatus();

foreach (var symbol in testSymbols)
{
    // dataClient.SubscribeTrade(symbol);
    // dataClient.SubscribeTicker(symbol);
    // dataClient.SubscribeKLine(symbol);
    // dataClient.SubscribeOrderbook(symbol);
}

await Task.Delay(2000);
// dataClient.UnsubscribeMarketStatus();
// await dataClient.CloseConnectionAsync();

foreach (var symbol in testSymbols)
{
    // dataClient.UnsubscribeTrade(symbol);
    // dataClient.UnsubscribeTicker(symbol);
    // dataClient.UnsubscribeKLine(symbol);
    // dataClient.UnsubscribeOrderbook(symbol);
}

Console.ReadKey();
Console.WriteLine("|====================================|");
Console.WriteLine("|              Stopped               |");
Console.WriteLine("|====================================|");


static void HandlePong(object? sender, PongEvent e) => Console.WriteLine($"Pong from server - {e}");
static void HandleError(object? sender, ErrorEvent e) => Console.WriteLine($"Error from server - {e}");
static void HandleMarketStatusSnapshot(object? sender, MarketStatusEvent e) => Console.WriteLine($"Market snapshot => {e}");
static void HandleMarketStatusUpdated(object? sender, MarketStatusEvent e) => Console.WriteLine($"Market updated => {e}");
static void HandleTradeSnapshot(object? sender, TradeEvent e) => Console.WriteLine($"Trade snapshot => {e}");
static void HandleTradeUpdated(object? sender, TradeEvent e) => Console.WriteLine($"Trade updated => {e}");
static void HandleTickerSnapshot(object? sender, TickerEvent e) => Console.WriteLine($"Ticker snapshot => {e}");
static void HandleTickerUpdated(object? sender, TickerEvent e) => Console.WriteLine($"Ticker update => {e}");
static void HandleKLineUpdated(object? sender, KLineEvent e) => Console.WriteLine($"KLine update => {e}");
static void HandleKLineSnapshot(object? sender, KLineEvent e) => Console.WriteLine($"KLine snapshot => {e}");
static void HandleOrderbookSnapshot(object? sender, OrderbookEvent e) => Console.WriteLine($"Orderbook snapshot => {e}");
static void HandleOrderbookUpdated(object? sender, OrderbookEvent e) => Console.WriteLine($"Orderbook update => {e}");
