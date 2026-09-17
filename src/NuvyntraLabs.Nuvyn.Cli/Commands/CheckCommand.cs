using System.CommandLine;
using NuvyntraLabs.Nuvyn.Cli.Scaffolding;

namespace NuvyntraLabs.Nuvyn.Cli.Commands;

internal static class CheckCommand
{
    public static Command Create()
    {
        var command = new Command("check", "Check that dotnet and the Nuvyn payload are available.");
        command.SetAction(_ =>
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
                var payload = Infrastructure.PayloadRoot.Resolve();
                ConsoleUi.Ok($"Payload: {payload}");
            }
            catch (Exception ex)
            {
                ConsoleUi.Error(ex.Message);
                ok = false;
            }

            return ok ? 0 : 1;
        });
        return command;
    }
}
