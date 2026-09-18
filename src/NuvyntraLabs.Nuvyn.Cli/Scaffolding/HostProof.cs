using System.Text.RegularExpressions;

namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

public sealed record HostProofReport(
    IReadOnlyList<string> Ok,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public bool Passed => Errors.Count == 0;
}

/// <summary>
/// Proves a Nuvyn host uses MVVMExpress + UIKit + the smallest Plugin.Maui.* set.
/// </summary>
public static class HostProof
{
    public static readonly string[] RequiredUseCalls =
    [
        "UseMvvmExpress",
        "UseNuvyntraUIKit",
        "UseHttpForge",
        "UseMauiFormValidation",
        "UseKeyboardManager",
    ];

    public static readonly string[] ForbiddenMauiProgram =
    [
        "AddGeneratedViewModels()",
        "UseMauiLocalStore",
        "Plugin.Maui.MVVMExpress.Generated",
    ];

    public static HostProofReport Inspect(string projectDir)
    {
        var ok = new List<string>();
        var errors = new List<string>();
        var warnings = new List<string>();

        var maui = HostScaffolder.FindMauiProject(projectDir);
        if (maui is null)
        {
            errors.Add("No MAUI host csproj (UseMaui) was found.");
            return new(ok, errors, warnings);
        }

        ok.Add($"MAUI host: {Path.GetFileName(maui)}");
        InspectMauiProgram(projectDir, ok, errors);
        InspectMainPage(projectDir, ok, errors);
        InspectStyles(Path.GetDirectoryName(maui)!, ok, errors);
        InspectPackages(maui, ok, errors, warnings);
        return new(ok, errors, warnings);
    }

    private static void InspectMauiProgram(string projectDir, List<string> ok, List<string> errors)
    {
        var program = Directory.EnumerateFiles(projectDir, "MauiProgram.cs", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (program is null)
        {
            errors.Add("MauiProgram.cs was not found.");
            return;
        }

        var text = File.ReadAllText(program);
        foreach (var call in RequiredUseCalls)
        {
            if (text.Contains(call, StringComparison.Ordinal))
                ok.Add($"MauiProgram: {call}");
            else
                errors.Add($"MauiProgram is missing {call}.");
        }

        foreach (var banned in ForbiddenMauiProgram)
        {
            if (text.Contains(banned, StringComparison.Ordinal))
                errors.Add($"MauiProgram must not contain {banned}.");
        }

        if (!text.Contains("AddTransient<MainPageViewModel>()", StringComparison.Ordinal))
            errors.Add("MauiProgram must register MainPageViewModel with AddTransient.");
        if (!text.Contains("AddTransient<MainPage>()", StringComparison.Ordinal))
            errors.Add("MauiProgram must register MainPage with AddTransient.");
    }

    private static void InspectMainPage(string projectDir, List<string> ok, List<string> errors)
    {
        var page = Directory.EnumerateFiles(projectDir, "MainPage.xaml", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (page is null)
        {
            errors.Add("Pages/MainPage.xaml was not found.");
            return;
        }

        var xaml = File.ReadAllText(page);
        if (xaml.Contains("NVHeading", StringComparison.Ordinal)
            && xaml.Contains("NVButton", StringComparison.Ordinal))
        {
            ok.Add("MainPage uses Lumina NV* controls.");
        }
        else
        {
            errors.Add("MainPage must use Lumina NVHeading / NVButton.");
        }

        if (Regex.IsMatch(xaml, @"<Button[\s>]") || Regex.IsMatch(xaml, @"<Label[\s>]"))
            errors.Add("MainPage must not use stock Button / Label.");
    }

    private static void InspectStyles(string mauiDir, List<string> ok, List<string> errors)
    {
        var colors = Path.Combine(mauiDir, "Resources", "Styles", "Colors.xaml");
        var styles = Path.Combine(mauiDir, "Resources", "Styles", "Styles.xaml");
        if (File.Exists(colors) && File.Exists(styles))
            ok.Add("Resources/Styles/Colors.xaml and Styles.xaml");
        else
            errors.Add("MAUI host is missing Resources/Styles/Colors.xaml or Styles.xaml.");
    }

    private static void InspectPackages(string mauiProject, List<string> ok, List<string> errors, List<string> warnings)
    {
        var ids = PackageIds(File.ReadAllText(mauiProject)).ToList();
        var nuvyntra = ids.Where(DefaultHostPackages.IsNuvyntraPackage).ToList();
        if (nuvyntra.Count == 0)
        {
            warnings.Add("MAUI csproj has no Nuvyntra PackageReference yet. Full nuvyn init adds them from nuget.org.");
            return;
        }

        foreach (var required in DefaultHostPackages.AfterHost)
        {
            if (nuvyntra.Contains(required, StringComparer.OrdinalIgnoreCase))
                ok.Add($"Package: {required}");
            else
                errors.Add($"MAUI csproj is missing {required}.");
        }

        foreach (var extra in nuvyntra.Where(DefaultHostPackages.IsUnexpectedHostPackage))
            errors.Add($"MAUI csproj has extra catalog package {extra}. Default host is the smallest set only.");
    }

    internal static IEnumerable<string> PackageIds(string csproj)
    {
        foreach (Match match in Regex.Matches(
                     csproj,
                     @"<PackageReference\s+Include=""([^""]+)""",
                     RegexOptions.IgnoreCase))
        {
            yield return match.Groups[1].Value;
        }
    }
}
