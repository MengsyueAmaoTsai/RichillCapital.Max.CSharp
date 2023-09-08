using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using RichillCapital.Max;

namespace Max.Wpf.Example;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly MaxDataClient _dataClient;

    public ObservableCollection<Log> Logs { get; }
    public bool ShouldAutoScroll { get; set; }

    public MainViewModel(MaxDataClient dataClient)
    {
        _dataClient = dataClient;
        _dataClient.Connected += _dataClient_Connected;
        
        Logs = new();
        BindingOperations.EnableCollectionSynchronization(Logs, new());
    }

    private async void _dataClient_Connected(object? sender, EventArgs e)
    {
        AddLog(Log.Info("Connected."));
        var markets = await _dataClient.GetMarketsAsync();
        AddLog(Log.Info($"Total markets: {markets.Count}"));
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
}
