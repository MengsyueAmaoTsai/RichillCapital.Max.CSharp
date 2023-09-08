using System;
using System.Linq;
using System.Windows;

namespace Max.Wpf.Example;


public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;
    }
    private void LogConsole_LayoutUpdated(object sender, EventArgs e)
    {
        var items = _viewModel.Logs;
        if (!_viewModel.ShouldAutoScroll || !items.Any())
            return;

        LogConsole.ScrollIntoView(items[items.Count - 1]);
        _viewModel.ShouldAutoScroll = false;
    }
}
