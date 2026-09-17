using NuvyntraLabs.Nuvyn.Cli.Agents;
using Xunit;
using NuvyntraLabs.Nuvyn.Cli.Commands;
using NuvyntraLabs.Nuvyn.Cli.Scaffolding;
using NuvyntraLabs.Nuvyn.Cli.Workflow;

namespace NuvyntraLabs.Nuvyn.Cli.Tests;

public sealed class InitTests
{
    [Fact]
    public void Init_skip_host_writes_nuvyn_tree_and_cursor_skills()
    {
        var root = NewTemp();
        var before = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(root);
        try
        {
            var code = InitCommand.Run("ClinicApp", "cursor", skipHost: true);
            Assert.Equal(0, code);

            var project = Path.Combine(root, "ClinicApp");
            Assert.True(File.Exists(Path.Combine(project, ".nuvyn", "constitution.md")));
            Assert.True(Directory.Exists(Path.Combine(project, "specs")));
            Assert.Contains("ClinicApp", File.ReadAllText(Path.Combine(project, ".nuvyn", "constitution.md")));

            foreach (var id in NuvynCommands.Ids)
            {
                var skill = Path.Combine(project, ".cursor", "skills", $"nuvyn-{id}", "SKILL.md");
                Assert.True(File.Exists(skill), skill);
                var text = File.ReadAllText(skill);
                Assert.Contains($"/nuvyn.{id}", text);
                Assert.Contains("handoffs:", text);
            }

            Assert.Contains("\"agent\": \"cursor\"", File.ReadAllText(Path.Combine(project, ".nuvyn", "init-options.json")));
        }
        finally
        {
            Directory.SetCurrentDirectory(before);
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData("copilot", ".github/skills/nuvyn-specify/SKILL.md")]
    [InlineData("claude", ".claude/commands/nuvyn.specify.md")]
    [InlineData("gemini", ".gemini/commands/nuvyn.specify.toml")]
    public void Agent_installer_writes_expected_path(string agentId, string relative)
    {
        var root = NewTemp();
        try
        {
            var agent = AiAgent.Find(agentId)!;
            AgentInstaller.Install(FindPayload(), root, agent);
            Assert.True(File.Exists(Path.Combine(root, relative)), relative);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void Default_host_is_five_packages_only()
    {
        Assert.Equal(
            [
                DefaultHostPackages.MvvmExpress,
                DefaultHostPackages.UiKit,
                DefaultHostPackages.HttpForge,
                DefaultHostPackages.FormValidation,
                DefaultHostPackages.KeyboardManager,
            ],
            DefaultHostPackages.AfterHost);
        Assert.DoesNotContain(DefaultHostPackages.LocalStore, DefaultHostPackages.AfterHost);
        Assert.DoesNotContain(DefaultHostPackages.NuvexaDb, DefaultHostPackages.AfterHost);
        Assert.Equal(
            [
                DefaultHostPackages.MvvmExpress,
                DefaultHostPackages.MvvmExpressDialogs,
                DefaultHostPackages.MvvmExpressNavigation,
                DefaultHostPackages.UiKit,
                DefaultHostPackages.HttpForge,
                DefaultHostPackages.FormValidation,
                DefaultHostPackages.KeyboardManager,
            ],
            DefaultHostPackages.MauiApp);
        Assert.Equal(
            [
                DefaultHostPackages.MvvmExpressCore,
                DefaultHostPackages.MvvmExpressSourceGenerators,
            ],
            DefaultHostPackages.Core);
        Assert.Equal([DefaultHostPackages.MvvmExpressTesting], DefaultHostPackages.Tests);
    }

    [Fact]
    public void Host_template_does_not_pin_nuvyntra_package_versions()
    {
        var host = Path.Combine(FindPayload(), "host");
        foreach (var project in Directory.GetFiles(host, "*.csproj", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(project);
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(
                         text,
                         @"<PackageReference\s+Include=""([^""]+)""",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                var id = match.Groups[1].Value;
                Assert.False(
                    DefaultHostPackages.IsNuvyntraPackage(id),
                    $"{Path.GetFileName(project)} includes {id}. Scaffold must add Nuvyntra packages at the latest nuget.org version.");
            }
        }
    }

    [Theory]
    [InlineData("NuvyntraLabs.UIKit", true)]
    [InlineData("Plugin.Maui.MVVMExpress", true)]
    [InlineData("Nuventra.NuvexaDB", true)]
    [InlineData("Microsoft.Maui.Controls", false)]
    public void IsNuvyntraPackage_matches_org_prefixes(string id, bool expected)
    {
        Assert.Equal(expected, DefaultHostPackages.IsNuvyntraPackage(id));
    }

    [Fact]
    public void ForProject_picks_maui_core_or_tests()
    {
        var dest = Path.Combine(NewTemp(), "out");
        Assert.True(HostTemplate.TryInstall(FindPayload(), dest, "ClinicApp"));

        var maui = Path.Combine(dest, "ClinicApp", "ClinicApp.csproj");
        var core = Path.Combine(dest, "ClinicApp.Core", "ClinicApp.Core.csproj");
        var tests = Path.Combine(dest, "ClinicApp.Tests", "ClinicApp.Tests.csproj");

        Assert.Equal(DefaultHostPackages.MauiApp, DefaultHostPackages.ForProject(maui));
        Assert.Equal(DefaultHostPackages.Core, DefaultHostPackages.ForProject(core));
        Assert.Equal(DefaultHostPackages.Tests, DefaultHostPackages.ForProject(tests));
    }

    [Fact]
    public void Maui_program_patch_adds_mvvmexpress()
    {
        var dir = Path.Combine(Path.GetTempPath(), "nuvyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(
                Path.Combine(dir, "MauiProgram.cs"),
                """
                using Microsoft.Extensions.Logging;
                using Microsoft.Maui.Controls.Hosting;
                using Microsoft.Maui.Hosting;

                namespace ClinicApp;

                public static class MauiProgram
                {
                    public static MauiApp CreateMauiApp()
                    {
                        var builder = MauiApp.CreateBuilder();
                        builder
                            .UseMauiApp<App>()
                            .ConfigureFonts(fonts => { });
                        return builder.Build();
                    }
                }
                """);

            Assert.True(MauiProgramPatcher.TryPatch(dir));
            var text = File.ReadAllText(Path.Combine(dir, "MauiProgram.cs"));
            Assert.Contains("UseMvvmExpress()", text);
            Assert.Contains("UseNuvyntraUIKit()", text);
            Assert.Contains("UseHttpForge()", text);
            Assert.Contains("UseMauiFormValidation()", text);
            Assert.Contains("UseKeyboardManager()", text);
            Assert.DoesNotContain("UseMauiLocalStore", text);
            Assert.DoesNotContain("AddGeneratedViewModels()", text);
            Assert.Contains("using Plugin.Maui.MVVMExpress;", text);
            Assert.Contains("using NuvyntraLabs.UIKit;", text);
        }
        finally
        {
            TryDelete(dir);
        }
    }

    [Fact]
    public void Maui_program_patch_does_not_duplicate_configured_use_mvvmexpress()
    {
        var dir = Path.Combine(Path.GetTempPath(), "nuvyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(
                Path.Combine(dir, "MauiProgram.cs"),
                """
                using NuvyntraLabs.UIKit;
                using Plugin.Maui.FormValidation;
                using Plugin.Maui.HttpForge;
                using Plugin.Maui.KeyboardManager;
                using Plugin.Maui.MVVMExpress.Dialogs;
                using Plugin.Maui.MVVMExpress.Hosting;
                using Plugin.Maui.MVVMExpress.Navigation;

                namespace ClinicApp;

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
                            .UseKeyboardManager();
                        return builder.Build();
                    }
                }
                """);

            MauiProgramPatcher.TryPatch(dir);
            var text = File.ReadAllText(Path.Combine(dir, "MauiProgram.cs"));
            Assert.DoesNotContain(".UseMvvmExpress()", text);
            Assert.DoesNotContain("AddGeneratedViewModels()", text);
            Assert.DoesNotContain("Plugin.Maui.MVVMExpress.Generated", text);
            Assert.Contains("UseMvvmExpress(o => o", text);
        }
        finally
        {
            TryDelete(dir);
        }
    }

    [Fact]
    public void Host_template_has_single_uikit_main_page()
    {
        var pages = Path.Combine(FindPayload(), "host", "MauiApp1", "Pages");
        var names = Directory.GetFiles(pages).Select(Path.GetFileName).OfType<string>().OrderBy(n => n).ToArray();
        Assert.Equal(new[] { "MainPage.xaml", "MainPage.xaml.cs" }, names);

        var home = File.ReadAllText(Path.Combine(pages, "MainPage.xaml"));
        Assert.Contains("nuvyntra.png", home);
        Assert.Contains("NVHeading", home);
        Assert.Contains("Increase", home);
        Assert.Contains("Decrease", home);
        Assert.Contains("NVButton", home);
        Assert.DoesNotContain("<Button", home);
        Assert.DoesNotContain("<Label", home);
        Assert.False(File.Exists(Path.Combine(pages, "LoginPage.xaml")));
    }

    [Fact]
    public void Host_template_install_rewrites_project_name()
    {
        var dest = Path.Combine(NewTemp(), "out");
        Assert.True(HostTemplate.TryInstall(FindPayload(), dest, "ClinicApp"));

        var main = Path.Combine(dest, "ClinicApp", "Pages", "MainPage.xaml");
        Assert.True(File.Exists(main), main);
        Assert.False(Directory.Exists(Path.Combine(dest, "MauiApp1")));
        Assert.False(File.Exists(Path.Combine(dest, "ClinicApp", "Pages", "LoginPage.xaml")));

        var xaml = File.ReadAllText(main);
        Assert.Contains("NVButton", xaml);
        Assert.Contains("x:Class=\"ClinicApp.MainPage\"", xaml);
        Assert.DoesNotContain("MauiApp1", xaml);

        var program = File.ReadAllText(Path.Combine(dest, "ClinicApp", "MauiProgram.cs"));
        Assert.Contains("namespace ClinicApp;", program);
        Assert.Contains("UseNuvyntraUIKit()", program);
        Assert.Contains("AddTransient<MainPageViewModel>()", program);
        Assert.Contains("AddTransient<MainPage>()", program);
        Assert.DoesNotContain("LoginPage", program);
    }

    [Fact]
    public void Rejects_bad_project_name()
    {
        var code = InitCommand.Run("1bad", "cursor", skipHost: true);
        Assert.Equal(1, code);
    }

    [Fact]
    public void Rejects_existing_directory()
    {
        var root = NewTemp();
        var before = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(root);
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "ClinicApp"));
            var code = InitCommand.Run("ClinicApp", "cursor", skipHost: true);
            Assert.Equal(1, code);
        }
        finally
        {
            Directory.SetCurrentDirectory(before);
            TryDelete(root);
        }
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

    private static void TryDelete(string dir)
    {
        try { Directory.Delete(dir, recursive: true); } catch { /* ignore locked temp files */ }
    }
}
