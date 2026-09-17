using Plugin.Maui.MVVMExpress.ComponentModel;
using Plugin.Maui.MVVMExpress.Dialogs;
using Plugin.Maui.MVVMExpress.Hosting;
using Plugin.Maui.MVVMExpress.Input;
using Plugin.Maui.MVVMExpress.Navigation;

namespace MauiApp1;

[RegisterViewModel]
[Route("main")]
public partial class MainPageViewModel : PageViewModel
{
    [Notify] private int _count;

    public MainPageViewModel(INavigator navigator, IDialogs dialogs)
        : base(navigator, dialogs)
    {
    }

    [AsyncModelCommand]
    private Task IncreaseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Count++;
        return Task.CompletedTask;
    }

    [AsyncModelCommand]
    private Task DecreaseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Count--;
        return Task.CompletedTask;
    }
}
