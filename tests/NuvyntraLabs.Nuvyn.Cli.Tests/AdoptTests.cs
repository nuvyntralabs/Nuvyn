using NuvyntraLabs.Nuvyn.Cli.Commands;
using NuvyntraLabs.Nuvyn.Cli.Infrastructure;
using NuvyntraLabs.Nuvyn.Cli.Scaffolding;
using NuvyntraLabs.Nuvyn.Cli.Workflow;
using Xunit;

namespace NuvyntraLabs.Nuvyn.Cli.Tests;

public sealed class AdoptTests
{
    [Fact]
    public void Adopt_writes_workflow_and_leaves_host_code_alone()
    {
        var root = NewTemp();
        var before = Directory.GetCurrentDirectory();
        try
        {
            var app = WriteStockMauiApp(Path.Combine(root, "FieldApp"), "FieldApp");
            var program = File.ReadAllText(Path.Combine(app, "FieldApp", "MauiProgram.cs"));
            var csproj = File.ReadAllText(Path.Combine(app, "FieldApp", "FieldApp.csproj"));
            var page = File.ReadAllText(Path.Combine(app, "FieldApp", "MainPage.xaml"));

            Directory.SetCurrentDirectory(app);
            Assert.Equal(0, AdoptCommand.Run("cursor", path: null, startDirectory: app));

            Assert.True(File.Exists(Path.Combine(app, ".nuvyn", "constitution.md")));
            Assert.True(File.Exists(Path.Combine(app, ".nuvyn", "adopt-report.md")));
            Assert.True(Directory.Exists(Path.Combine(app, "specs")));
            foreach (var id in NuvynCommands.Ids)
                Assert.True(File.Exists(Path.Combine(app, ".cursor", "skills", $"nuvyn-{id}", "SKILL.md")));

            var options = InitOptions.TryRead(app);
            Assert.NotNull(options);
            Assert.Equal(InitOptions.ModeAdopt, options.Mode);
            Assert.True(options.IsAdopt);
            Assert.Equal("cursor", options.Agent);

            var constitution = File.ReadAllText(Path.Combine(app, ".nuvyn", "constitution.md"));
            Assert.Contains("## Adopted host", constitution);
            Assert.Contains("Do not add or migrate to MVVMExpress", constitution);

            var report = File.ReadAllText(Path.Combine(app, ".nuvyn", "adopt-report.md"));
            Assert.Contains("CommunityToolkit.Mvvm", report);
            Assert.Contains("HttpClient", report);
            Assert.Contains("Plugin.Maui.MVVMExpress", report);
            Assert.Contains("Plugin.Maui.HttpForge", report);
            Assert.Contains("NuvyntraLabs.UIKit", report);
            Assert.Contains("AppShell", report);

            Assert.Equal(program, File.ReadAllText(Path.Combine(app, "FieldApp", "MauiProgram.cs")));
            Assert.Equal(csproj, File.ReadAllText(Path.Combine(app, "FieldApp", "FieldApp.csproj")));
            Assert.Equal(page, File.ReadAllText(Path.Combine(app, "FieldApp", "MainPage.xaml")));
            Assert.DoesNotContain("UseMvvmExpress", File.ReadAllText(Path.Combine(app, "FieldApp", "MauiProgram.cs")));
            Assert.DoesNotContain("NuvyntraLabs.UIKit", File.ReadAllText(Path.Combine(app, "FieldApp", "FieldApp.csproj")));
        }
        finally
        {
            Directory.SetCurrentDirectory(before);
            TryDelete(root);
        }
    }

    [Fact]
    public void Adopt_refuses_a_folder_that_is_not_maui()
    {
        var root = NewTemp();
        try
        {
            File.WriteAllText(Path.Combine(root, "notes.txt"), "no csproj");
            Assert.Equal(1, AdoptCommand.Run("cursor", path: root));
            Assert.False(Directory.Exists(Path.Combine(root, ".nuvyn")));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void Adopt_refuses_an_existing_nuvyn_project()
    {
        var root = NewTemp();
        var before = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(root);
        try
        {
            Assert.Equal(0, InitCommand.Run("ClinicApp", "cursor", skipHost: true));
            Assert.Equal(1, AdoptCommand.Run("cursor", path: Path.Combine(root, "ClinicApp")));
        }
        finally
        {
            Directory.SetCurrentDirectory(before);
            TryDelete(root);
        }
    }

    [Fact]
    public void Adopt_path_attaches_without_changing_cwd_host()
    {
        var root = NewTemp();
        try
        {
            var app = WriteStockMauiApp(Path.Combine(root, "HarborDesk"), "HarborDesk");
            Assert.Equal(0, AdoptCommand.Run("claude", path: app));
            Assert.True(File.Exists(Path.Combine(app, ".claude", "commands", "nuvyn.specify.md")));
            Assert.True(File.Exists(Path.Combine(app, ".nuvyn", "adopt-report.md")));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void Update_after_adopt_keeps_adopt_report_and_mode()
    {
        var root = NewTemp();
        try
        {
            var app = WriteStockMauiApp(Path.Combine(root, "FieldApp"), "FieldApp");
            Assert.Equal(0, AdoptCommand.Run("cursor", path: app));
            var report = Path.Combine(app, ".nuvyn", "adopt-report.md");
            File.WriteAllText(report, "# customized adopt report\n");

            Assert.Equal(0, UpdateCommand.Run(null, app));
            Assert.Equal("# customized adopt report\n", File.ReadAllText(report));
            Assert.True(InitOptions.TryRead(app)!.IsAdopt);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void Check_on_adopted_host_skips_greenfield_proof()
    {
        var root = NewTemp();
        try
        {
            var app = WriteStockMauiApp(Path.Combine(root, "FieldApp"), "FieldApp");
            Assert.Equal(0, AdoptCommand.Run("cursor", path: app));
            Assert.Equal(0, CheckCommand.Run(app));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void Inventory_records_stock_stack_without_forcing_nuvyntra()
    {
        var root = NewTemp();
        try
        {
            var app = WriteStockMauiApp(Path.Combine(root, "FieldApp"), "FieldApp");
            var inventory = AdoptInventory.TryInspect(app);
            Assert.NotNull(inventory);
            Assert.False(inventory.HasMvvmExpress);
            Assert.False(inventory.HasUiKit);
            Assert.False(inventory.HasHttpForge);
            Assert.Contains(inventory.Layers, layer => layer.Area == "Architecture" && layer.Found.Contains("CommunityToolkit.Mvvm"));
            Assert.Contains(inventory.Layers, layer => layer.Area == "HTTP" && layer.Found.Contains("HttpClient"));
            Assert.Contains(inventory.Layers, layer => layer.Area == "UI" && layer.DoNotAddUnlessAsked.Contains("Lumina"));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void Refresh_workflow_skips_adopt_report()
    {
        Assert.True(PayloadInstaller.IsUserOwnedWorkflowFile("adopt-report.md"));
    }

    private static string WriteStockMauiApp(string appRoot, string name)
    {
        var hostDir = Path.Combine(appRoot, name);
        Directory.CreateDirectory(hostDir);
        File.WriteAllText(
            Path.Combine(hostDir, $"{name}.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <UseMaui>true</UseMaui>
                <TargetFrameworks>net10.0-android</TargetFrameworks>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
                <PackageReference Include="CommunityToolkit.Maui" Version="12.0.0" />
              </ItemGroup>
            </Project>
            """);
        File.WriteAllText(
            Path.Combine(hostDir, "MauiProgram.cs"),
            $$"""
            using Microsoft.Maui.Controls;
            namespace {{name}};
            public static class MauiProgram
            {
                public static MauiApp CreateMauiApp()
                {
                    var builder = MauiApp.CreateBuilder();
                    builder.UseMauiApp<App>();
                    builder.Services.AddSingleton<HttpClient>();
                    return builder.Build();
                }
            }
            """);
        File.WriteAllText(Path.Combine(hostDir, "AppShell.xaml"), """<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui" />""");
        File.WriteAllText(
            Path.Combine(hostDir, "MainPage.xaml"),
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
              <VerticalStackLayout>
                <Label Text="Hello" />
                <Button Text="Go" />
              </VerticalStackLayout>
            </ContentPage>
            """);
        return appRoot;
    }

    private static string NewTemp()
    {
        var dir = Path.Combine(Path.GetTempPath(), "nuvyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void TryDelete(string dir)
    {
        try { Directory.Delete(dir, recursive: true); } catch { /* ignore locked temp files */ }
    }
}
