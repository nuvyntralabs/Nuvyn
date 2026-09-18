using NuvyntraLabs.Nuvyn.Cli.Scaffolding;
using Xunit;

namespace NuvyntraLabs.Nuvyn.Cli.Tests;

public sealed class HostProofTests
{
    [Fact]
    public void Embedded_host_uses_mvvmexpress_uikit_and_the_smallest_plugin_set()
    {
        var dest = Path.Combine(NewTemp(), "out");
        Assert.True(HostTemplate.TryInstall(FindPayload(), dest, "HarborDesk"));

        Assert.True(File.Exists(Path.Combine(dest, "HarborDesk", "Resources", "Styles", "Colors.xaml")));
        Assert.True(File.Exists(Path.Combine(dest, "HarborDesk", "Resources", "Styles", "Styles.xaml")));

        var report = HostProof.Inspect(dest);
        Assert.True(report.Passed, string.Join(Environment.NewLine, report.Errors));
        Assert.Contains(report.Ok, line => line.Contains("UseMvvmExpress", StringComparison.Ordinal));
        Assert.Contains(report.Ok, line => line.Contains("UseNuvyntraUIKit", StringComparison.Ordinal));
        Assert.Contains(report.Ok, line => line.Contains("Lumina NV*", StringComparison.Ordinal));
        Assert.Contains(report.Warnings, line => line.Contains("no Nuvyntra PackageReference", StringComparison.Ordinal));
    }

    [Fact]
    public void Host_proof_rejects_a_catalog_dump_in_the_maui_csproj()
    {
        var dest = Path.Combine(NewTemp(), "out");
        Assert.True(HostTemplate.TryInstall(FindPayload(), dest, "HarborDesk"));

        var csproj = Path.Combine(dest, "HarborDesk", "HarborDesk.csproj");
        var text = File.ReadAllText(csproj);
        text = text.Replace(
            "</Project>",
            """
              <ItemGroup>
                <PackageReference Include="Plugin.Maui.MVVMExpress" />
                <PackageReference Include="Plugin.Maui.MVVMExpress.Dialogs" />
                <PackageReference Include="Plugin.Maui.MVVMExpress.Navigation" />
                <PackageReference Include="NuvyntraLabs.UIKit" />
                <PackageReference Include="Plugin.Maui.HttpForge" />
                <PackageReference Include="Plugin.Maui.FormValidation" />
                <PackageReference Include="Plugin.Maui.KeyboardManager" />
                <PackageReference Include="Plugin.Maui.LocalStore" />
                <PackageReference Include="Nuventra.NuvexaDB" />
                <PackageReference Include="Plugin.Maui.AppLock" />
              </ItemGroup>
            </Project>
            """,
            StringComparison.Ordinal);
        File.WriteAllText(csproj, text);

        var report = HostProof.Inspect(dest);
        Assert.False(report.Passed);
        Assert.Contains(report.Errors, line => line.Contains("Plugin.Maui.LocalStore", StringComparison.Ordinal));
        Assert.Contains(report.Errors, line => line.Contains("Nuventra.NuvexaDB", StringComparison.Ordinal));
        Assert.Contains(report.Errors, line => line.Contains("Plugin.Maui.AppLock", StringComparison.Ordinal));
    }

    [Fact]
    public void Host_proof_accepts_the_smallest_maui_package_set()
    {
        var dest = Path.Combine(NewTemp(), "out");
        Assert.True(HostTemplate.TryInstall(FindPayload(), dest, "HarborDesk"));

        var csproj = Path.Combine(dest, "HarborDesk", "HarborDesk.csproj");
        var text = File.ReadAllText(csproj);
        text = text.Replace(
            "</Project>",
            """
              <ItemGroup>
                <PackageReference Include="Plugin.Maui.MVVMExpress" />
                <PackageReference Include="Plugin.Maui.MVVMExpress.Dialogs" />
                <PackageReference Include="Plugin.Maui.MVVMExpress.Navigation" />
                <PackageReference Include="NuvyntraLabs.UIKit" />
                <PackageReference Include="Plugin.Maui.HttpForge" />
                <PackageReference Include="Plugin.Maui.FormValidation" />
                <PackageReference Include="Plugin.Maui.KeyboardManager" />
              </ItemGroup>
            </Project>
            """,
            StringComparison.Ordinal);
        File.WriteAllText(csproj, text);

        var report = HostProof.Inspect(dest);
        Assert.True(report.Passed, string.Join(Environment.NewLine, report.Errors));
        Assert.Empty(report.Warnings);
        foreach (var package in DefaultHostPackages.AfterHost)
            Assert.Contains(report.Ok, line => line.Contains(package, StringComparison.Ordinal));
    }

    [Fact]
    public void Unexpected_host_package_is_any_catalog_id_outside_the_maui_default_set()
    {
        Assert.True(DefaultHostPackages.IsUnexpectedHostPackage(DefaultHostPackages.LocalStore));
        Assert.True(DefaultHostPackages.IsUnexpectedHostPackage(DefaultHostPackages.NuvexaDb));
        Assert.False(DefaultHostPackages.IsUnexpectedHostPackage(DefaultHostPackages.UiKit));
        Assert.False(DefaultHostPackages.IsUnexpectedHostPackage(DefaultHostPackages.MvvmExpressDialogs));
    }

    private static string NewTemp()
    {
        var dir = Path.Combine(Path.GetTempPath(), "nuvyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string FindPayload()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var payload = Path.Combine(dir.FullName, "payload");
            if (Directory.Exists(Path.Combine(payload, "commands")))
                return payload;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("payload/");
    }
}
