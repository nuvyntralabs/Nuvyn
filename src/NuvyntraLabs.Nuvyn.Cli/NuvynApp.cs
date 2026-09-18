using System.CommandLine;
using NuvyntraLabs.Nuvyn.Cli.Commands;

namespace NuvyntraLabs.Nuvyn.Cli;

public static class NuvynApp
{
    public static Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
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

        return root.Parse(args).InvokeAsync(cancellationToken: cancellationToken);
    }
}
