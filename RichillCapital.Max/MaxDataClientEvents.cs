using RichillCapital.Max.Events;

namespace RichillCapital.Max;

public sealed partial class MaxDataClient
{
    public event EventHandler? Connected;
    public event EventHandler? Disconnect;
    public event EventHandler? MarketStatusUpdate;
    public event EventHandler? MarketStatusSnapshot;
    public event EventHandler<TradeUpdatedEvent>? TradeUpdate;
    public event EventHandler? TradeSnapshot;
    public event EventHandler? TickerUpdate;
    public event EventHandler? TickerSnapshot;
    public event EventHandler? KLineUpdate;
    public event EventHandler? KLineSnapshot;
    public event EventHandler? OrderbookUpdate;
    public event EventHandler? OrderbookSnapshot;
}
