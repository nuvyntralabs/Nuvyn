namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

internal static class MauiProgramPatcher
{
    public static bool TryPatch(string projectDir)
    {
        var path = Directory.EnumerateFiles(projectDir, "MauiProgram.cs", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (path is null)
            return false;

        var text = File.ReadAllText(path);
        var original = text;

        text = EnsureUsing(text, "Plugin.Maui.MVVMExpress");
        text = EnsureUsing(text, "NuvyntraLabs.UIKit");
        text = EnsureUsing(text, "Plugin.Maui.HttpForge");
        text = EnsureUsing(text, "Plugin.Maui.FormValidation");
        text = EnsureUsing(text, "Plugin.Maui.KeyboardManager");

        const string hook = ".UseMauiApp<App>()";
        var hasConfiguredMvvm = text.Contains("UseMvvmExpress", StringComparison.Ordinal);

        if (text.Contains(hook, StringComparison.Ordinal) && !hasConfiguredMvvm)
        {
            text = text.Replace(
                hook,
                """
                .UseMauiApp<App>()
                    .UseMvvmExpress()
                    .UseNuvyntraUIKit()
                    .UseHttpForge()
                    .UseMauiFormValidation()
                    .UseKeyboardManager()
                """,
                StringComparison.Ordinal);
        }
        else
        {
            text = EnsureChainCall(text, "UseNuvyntraUIKit()");
            text = EnsureChainCall(text, "UseHttpForge()");
            text = EnsureChainCall(text, "UseMauiFormValidation()");
            text = EnsureChainCall(text, "UseKeyboardManager()");
        }

        if (text == original)
            return false;

        File.WriteAllText(path, text);
        return true;
    }

    private static string EnsureChainCall(string text, string call)
    {
        var name = call.EndsWith("()", StringComparison.Ordinal)
            ? call[..^2]
            : call;
        if (text.Contains(name, StringComparison.Ordinal))
            return text;

        const string hook = ".UseMauiApp<App>()";
        if (!text.Contains(hook, StringComparison.Ordinal))
            return text;

        return text.Replace(hook, $".UseMauiApp<App>()\n            .{call}", StringComparison.Ordinal);
    }

    private static string EnsureUsing(string text, string ns)
    {
        var line = $"using {ns};";
        if (text.Contains(line, StringComparison.Ordinal))
            return text;

        var idx = text.LastIndexOf("using ", StringComparison.Ordinal);
        if (idx < 0)
            return line + Environment.NewLine + text;

        var end = text.IndexOf('\n', idx);
        if (end < 0)
            return text + Environment.NewLine + line;

        return text.Insert(end + 1, line + Environment.NewLine);
    }
}
