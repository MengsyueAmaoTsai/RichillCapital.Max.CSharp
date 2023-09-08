using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Max.Wpf.Example.Models;

using RichillCapital.Max;
using RichillCapital.Max.Models;

namespace Max.Wpf.Example;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly MaxDataClient _dataClient;

    [ObservableProperty]
    private MarketResponse? _selectedMarket = null;

    public ObservableCollection<Log> Logs { get; }

    public bool ShouldAutoScroll { get; set; }

    public MainViewModel(MaxDataClient dataClient)
    {
        _dataClient = dataClient;

        Logs = new();
        BindingOperations.EnableCollectionSynchronization(Logs, new());
    }


    [RelayCommand]
    public async Task EstablishConnectionAsync()
    {
        try
        {
            AddLog(Log.Info($"Connecting to {_dataClient.Id}"));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}");
        }
    }

    private void AddLog(Log log)
    {
        Logs.Add(log);
        ShouldAutoScroll = true;
    }
}
