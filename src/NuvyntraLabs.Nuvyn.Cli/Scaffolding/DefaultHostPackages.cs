namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

/// <summary>
/// Packages <c>nuvyn init</c> always adds. Do not add any other PackageReference
/// until the user explicitly asks (plan / implement). Versions are never pinned —
/// <c>dotnet add package</c> without <c>--version</c> takes the latest nuget.org release.
/// </summary>
public static class DefaultHostPackages
{
    public const string MvvmExpress = "Plugin.Maui.MVVMExpress";
    public const string MvvmExpressDialogs = "Plugin.Maui.MVVMExpress.Dialogs";
    public const string MvvmExpressNavigation = "Plugin.Maui.MVVMExpress.Navigation";
    public const string MvvmExpressCore = "Plugin.Maui.MVVMExpress.Core";
    public const string MvvmExpressSourceGenerators = "Plugin.Maui.MVVMExpress.SourceGenerators";
    public const string MvvmExpressTesting = "Plugin.Maui.MVVMExpress.Testing";
    public const string UiKit = "NuvyntraLabs.UIKit";
    public const string HttpForge = "Plugin.Maui.HttpForge";
    public const string FormValidation = "Plugin.Maui.FormValidation";
    public const string KeyboardManager = "Plugin.Maui.KeyboardManager";

    public const string LocalStore = "Plugin.Maui.LocalStore";
    public const string NuvexaDb = "Nuventra.NuvexaDB";

    /// <summary>Default host product packages (MAUI app).</summary>
    public static readonly IReadOnlyList<string> AfterHost =
    [
        MvvmExpress,
        UiKit,
        HttpForge,
        FormValidation,
        KeyboardManager,
    ];

    /// <summary>Every Nuvyntra package the MAUI app project receives.</summary>
    public static readonly IReadOnlyList<string> MauiApp =
    [
        MvvmExpress,
        MvvmExpressDialogs,
        MvvmExpressNavigation,
        UiKit,
        HttpForge,
        FormValidation,
        KeyboardManager,
    ];

    /// <summary>Nuvyntra packages the Core (ViewModel) project receives.</summary>
    public static readonly IReadOnlyList<string> Core =
    [
        MvvmExpressCore,
        MvvmExpressSourceGenerators,
    ];

    /// <summary>Nuvyntra packages the test project receives.</summary>
    public static readonly IReadOnlyList<string> Tests =
    [
        MvvmExpressTesting,
    ];

    public static bool IsNuvyntraPackage(string packageId)
    {
        return packageId.StartsWith("NuvyntraLabs.", StringComparison.OrdinalIgnoreCase)
            || packageId.StartsWith("Plugin.Maui.", StringComparison.OrdinalIgnoreCase)
            || packageId.StartsWith("Plugin.Wpf.", StringComparison.OrdinalIgnoreCase)
            || packageId.StartsWith("Plugin.WinUI.", StringComparison.OrdinalIgnoreCase)
            || packageId.StartsWith("Plugin.Avalonia.", StringComparison.OrdinalIgnoreCase)
            || packageId.StartsWith("Plugin.Uno.", StringComparison.OrdinalIgnoreCase)
            || packageId.StartsWith("Nuventra.", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsAllowedOnMauiApp(string packageId) =>
        MauiApp.Contains(packageId, StringComparer.OrdinalIgnoreCase);

    public static bool IsUnexpectedHostPackage(string packageId) =>
        IsNuvyntraPackage(packageId) && !IsAllowedOnMauiApp(packageId);

    public static IReadOnlyList<string> ForProject(string projectPath)
    {
        var name = Path.GetFileName(projectPath);
        if (name.EndsWith(".Tests.csproj", StringComparison.OrdinalIgnoreCase))
            return Tests;
        if (name.EndsWith(".Core.csproj", StringComparison.OrdinalIgnoreCase))
            return Core;
        if (File.Exists(projectPath) &&
            File.ReadAllText(projectPath).Contains("UseMaui", StringComparison.Ordinal))
            return MauiApp;
        return [];
    }
}
