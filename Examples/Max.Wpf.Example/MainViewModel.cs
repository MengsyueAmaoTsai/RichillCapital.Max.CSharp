using System;
using System.Collections.ObjectModel;
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

    [ObservableProperty]
    private MarketResponse? _selectedMarket = null;

    public ObservableCollection<Log> Logs { get; }
    public ObservableCollection<Log> Trades { get; }
    public ObservableCollection<MarketResponse> Markets { get; }
    public bool ShouldAutoScroll { get; set; }

    public MainViewModel(MaxDataClient dataClient)
    {
        _dataClient = dataClient;
        _dataClient.Connected += _dataClient_Connected;
        _dataClient.TickerUpdate += _dataClient_TickerUpdate;
        _dataClient.TradeUpdate += _dataClient_TradeUpdate;
        
        Logs = new();
        BindingOperations.EnableCollectionSynchronization(Logs, new());
        Markets = new();
        BindingOperations.EnableCollectionSynchronization(Markets, new());
        Trades = new();
        BindingOperations.EnableCollectionSynchronization(Trades, new());
    }

    private void _dataClient_TradeUpdate(object? sender, TradeUpdatedEvent e)
    {
        AddLog(Log.Info($"{e.MarketId} {e.DateTime}"));
    }

    private void _dataClient_TickerUpdate(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private async void _dataClient_Connected(object? sender, EventArgs e)
    {
        AddLog(Log.Info("Connected."));
        await LoadMarketsAsync();
        SubscribeTrades();
    }

    [RelayCommand(CanExecute = nameof(CanEstablishConnection))]
    public async Task EstablishConnectionAsync()
    {
        try
        {
            AddLog(Log.Info($"Connecting to {_dataClient.Id}"));
            await _dataClient.EstablishConnectionAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}");
        }
    }

    private bool CanEstablishConnection() => !_dataClient.IsConnected;

    private void AddLog(Log log)
    {
        Logs.Add(log);
        ShouldAutoScroll = true;
    }

    private async Task LoadMarketsAsync()
    {
        Markets.Clear();
        var markets = await _dataClient.GetMarketsAsync();
        foreach (var market in markets)
        {
            Markets.Add(market);
        }
        AddLog(Log.Info($"Markets loaded. Total markets: {markets.Count}"));
    }

    private void SubscribeTrades()
    {
        foreach (var market in Markets)
        {
            _dataClient.SubscribeTrade(market.Id);
            AddLog(Log.Info($"Subscribe to market {market.Id}"));
        }
    }
}
