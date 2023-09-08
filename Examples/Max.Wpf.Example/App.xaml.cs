using System;
using System.Text;
using System.Windows;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RichillCapital.Max;

namespace Max.Wpf.Example;

public partial class App : Application
{

    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; private set; }
    public App() => Services = ConfigureServices();

    protected override void OnStartup(StartupEventArgs e)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        base.OnStartup(e);
        var window = Services.GetRequiredService<MainWindow>();
        window.Show();
    }

    private static IServiceProvider ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<MaxDataClient>()
            .AddSingleton<WeakReferenceMessenger>()
            .AddSingleton<IMessenger, WeakReferenceMessenger>(provider =>
                provider.GetRequiredService<WeakReferenceMessenger>())
            .AddSingleton<MainWindow>()
            .AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }
}
