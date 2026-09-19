using System.CommandLine;
using NuvyntraLabs.Nuvyn.Cli.Commands;

namespace NuvyntraLabs.Nuvyn.Cli;

public static class NuvynApp
{
    public static Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var remaining = ToolUpdateCheck.ConsumeNoUpdateCheckFlag(args, out var noUpdateCheck);
        var update = ToolUpdateCheck.Run(new UpdateCheckOptions
        {
            ToolKey = "nuvyn",
            PackageId = "NuvyntraLabs.Nuvyn.Cli",
            CurrentVersion = ToolUpdateCheck.ReadAssemblyVersion(typeof(NuvynApp)),
            Args = remaining,
            Stdout = Console.Out,
            Stderr = Console.Error,
            Stdin = Console.In,
            AllowPrompt = ToolUpdateCheck.IsInteractive(Console.Out) && !noUpdateCheck,
        });
        if (update == UpdateCheckOutcome.UpdatedExit)
            return Task.FromResult(0);

        var root = new RootCommand("Nuvyn — spec-driven .NET MAUI apps on the Nuvyntra stack.")
        {
            InitCommand.Create(),
            UpdateCommand.Create(),
            VersionCommand.Create(),
            CheckCommand.Create(),
        };

        root.SetAction(_ =>
        {
            ConsoleUi.Banner();
            ConsoleUi.Info("Run 'nuvyn --help' for usage. Start with 'nuvyn init <project_name>'.");
            return 0;
        });

        return root.Parse(remaining).InvokeAsync(cancellationToken: cancellationToken);
    }
}
