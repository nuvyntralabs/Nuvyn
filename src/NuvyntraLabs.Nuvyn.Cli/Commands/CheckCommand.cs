using System.CommandLine;
using NuvyntraLabs.Nuvyn.Cli.Infrastructure;
using NuvyntraLabs.Nuvyn.Cli.Scaffolding;

namespace NuvyntraLabs.Nuvyn.Cli.Commands;

internal static class CheckCommand
{
    public static Command Create()
    {
        var command = new Command(
            "check",
            "Check that dotnet, the Nuvyn payload, and (when inside a project) the host stack are available.");
        command.SetAction(_ => Run());
        return command;
    }

    internal static int Run(string? startDirectory = null)
    {
        ConsoleUi.Banner();
        var ok = true;

        if (HostScaffolder.TryDotnet(["--version"], Directory.GetCurrentDirectory()))
            ConsoleUi.Ok("dotnet is on PATH");
        else
        {
            ConsoleUi.Error("dotnet was not found. Install the .NET SDK.");
            ok = false;
        }

        try
        {
            var payload = PayloadRoot.Resolve();
            ConsoleUi.Ok($"Payload: {payload}");
        }
        catch (Exception ex)
        {
            ConsoleUi.Error(ex.Message);
            ok = false;
        }

        var project = ProjectRoot.TryFind(startDirectory);
        if (project is null)
        {
            ConsoleUi.Info("Not inside a Nuvyn app. Host proof runs after nuvyn init.");
            return ok ? 0 : 1;
        }

        ConsoleUi.Ok($"Project: {project}");
        var proof = HostProof.Inspect(project);
        foreach (var item in proof.Ok)
            ConsoleUi.Ok(item);
        foreach (var warning in proof.Warnings)
            ConsoleUi.Warn(warning);
        foreach (var error in proof.Errors)
        {
            ConsoleUi.Error(error);
            ok = false;
        }

        return ok ? 0 : 1;
    }
}
