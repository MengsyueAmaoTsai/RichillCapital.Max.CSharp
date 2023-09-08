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
        SetWindowSizeToPercentageOfScreen(80);
    }

    private void SetWindowSizeToPercentageOfScreen(double percentage)
    {
        Width = SystemParameters.PrimaryScreenWidth * (percentage * 0.01);
        Height = SystemParameters.PrimaryScreenHeight * (percentage * 0.01);
    }

    private void LogConsole_LayoutUpdated(object sender, EventArgs e)
    {
        var items = _viewModel.Logs;
        if (!_viewModel.ShouldAutoScroll || !items.Any())
            return;

        LogConsole.ScrollIntoView(items[^1]);
        _viewModel.ShouldAutoScroll = false;
    }
}
