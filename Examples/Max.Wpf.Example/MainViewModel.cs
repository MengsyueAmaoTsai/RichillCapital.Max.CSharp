using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Max.Wpf.Example.Models;

using RichillCapital.Max;
using RichillCapital.Max.Events;
using RichillCapital.Max.Models;

namespace Max.Wpf.Example;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly MaxDataClient _dataClient;
    private readonly Dictionary<string, List<Trade>> _tradeCache = new();

    [ObservableProperty]
    private MarketResponse? _selectedMarket = null;

    public ObservableCollection<Log> Logs { get; } = new();
    public ObservableCollection<MarketResponse> Markets { get; } = new();
    public ObservableCollection<Trade> DisplayTrades { get; } = new();
    public ObservableCollection<Order> Orders { get; } = new();
    public ObservableCollection<Execution> Executions { get; } = new();
    public ObservableCollection<Position> Positions { get; } = new();
    public ObservableCollection<MyTrade> MyTrades { get; } = new();

    public bool ShouldAutoScroll { get; set; }

    public MainViewModel(MaxDataClient dataClient)
    {
        _dataClient = dataClient;
        _dataClient.Pong += _dataClient_Pong;
        _dataClient.TradeUpdated += _dataClient_TradeUpdated;

        BindingOperations.EnableCollectionSynchronization(Logs, new());
        BindingOperations.EnableCollectionSynchronization(Markets, new());
        BindingOperations.EnableCollectionSynchronization(DisplayTrades, new());
        BindingOperations.EnableCollectionSynchronization(Orders, new());
        BindingOperations.EnableCollectionSynchronization(Executions, new());
        BindingOperations.EnableCollectionSynchronization(Positions, new());
        BindingOperations.EnableCollectionSynchronization(MyTrades, new());
    }

    private void _dataClient_TradeUpdated(object? sender, TradeEvent e)
    {
        var symbol = e.MarketId;

        foreach (var data in e.Trades)
        {
            AddLog(Log.Info($"{symbol} {data}"));

            var trade = new Trade()
            {
                Symbol = symbol,
                Time = data.Time,
                Price = data.Price,
                Volume = data.Volume,
            };
            _tradeCache[symbol].Add(trade);

            if (SelectedMarket is not null && SelectedMarket.Id == symbol) DisplayTrades.Add(trade);
        }
    }

    private void _dataClient_Pong(object? sender, PongEvent e) => AddLog(Log.Info($"Pong from server"));

    [RelayCommand]
    public async Task EstablishConnectionAsync()
    {
        try
        {
            AddLog(Log.Info($"Connecting to {nameof(MaxDataClient)}..."));
            var serverTime = await _dataClient.GetServerTimeAsync();

            AddLog(Log.Info($"Server time: {serverTime}"));
            await _dataClient.EstablishConnectionAsync();
            await FetchMarketsAsync();

            SubscribeAllMarketData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}");
        }
    }

    private async Task FetchMarketsAsync()
    {
        AddLog(Log.Info($"Fetching markets data..."));
        var markets = await _dataClient.GetMarketsAsync();
        _dataClient.SubscribeMarketStatus();
        foreach (var market in markets)
        {
            Markets.Add(market);
            _tradeCache[market.Id] = new List<Trade>();
        }
        SelectedMarket = Markets.First();
        AddLog(Log.Info($"Fetching markets data completed. Total markets: {Markets.Count}."));
    }

    private void AddLog(Log log)
    {
        Logs.Add(log);
        ShouldAutoScroll = true;
    }

    private void SubscribeAllMarketData()
    {
        foreach (var market in Markets)
        {
            AddLog(Log.Info($"Subscribe to {market.Id}..."));
            _dataClient.SubscribeTrade(market.Id);
        }
    }

    partial void OnSelectedMarketChanged(MarketResponse? market)
    {
        if (market is null) return;

        DisplayTrades.Clear();

        foreach (var trade in _tradeCache[market.Id])
        {
            DisplayTrades.Add(trade);
        }
    }
}
