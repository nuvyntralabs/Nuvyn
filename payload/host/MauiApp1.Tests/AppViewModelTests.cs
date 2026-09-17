using MauiApp1;
using Plugin.Maui.MVVMExpress.Navigation;
using Plugin.Maui.MVVMExpress.Testing;
using Xunit;

namespace MauiApp1.Tests;

public sealed class AppViewModelTests
{
    [Fact]
    public async Task Increase_and_decrease_update_count()
    {
        var vm = new MainPageViewModel(new InMemoryNavigator(), new FakeDialogs());
        Assert.Equal(0, vm.Count);

        await vm.IncreaseCommand.ExecuteAsync();
        Assert.Equal(1, vm.Count);

        await vm.DecreaseCommand.ExecuteAsync();
        Assert.Equal(0, vm.Count);
    }
}
