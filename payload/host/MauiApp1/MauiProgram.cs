using Microsoft.Extensions.Logging;
using NuvyntraLabs.UIKit;
using Plugin.Maui.FormValidation;
using Plugin.Maui.HttpForge;
using Plugin.Maui.KeyboardManager;
using Plugin.Maui.MVVMExpress.Dialogs;
using Plugin.Maui.MVVMExpress.Hosting;
using Plugin.Maui.MVVMExpress.Navigation;

namespace MauiApp1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMvvmExpress(o => o
                .UseNavigationPage((nav, _) => nav
                    .Map<MainPageViewModel, MainPage>("main"))
                .UseDialogs())
            .UseNuvyntraUIKit()
            .UseHttpForge()
            .UseMauiFormValidation()
            .UseKeyboardManager()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
