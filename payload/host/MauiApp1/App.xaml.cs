namespace MauiApp1;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(new NavigationPage(_services.GetRequiredService<MainPage>()));
}
