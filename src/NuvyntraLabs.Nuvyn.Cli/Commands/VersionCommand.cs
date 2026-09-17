using System.CommandLine;
using System.Reflection;

namespace NuvyntraLabs.Nuvyn.Cli.Commands;

internal static class VersionCommand
{
    public static Command Create()
    {
        var command = new Command("version", "Show the Nuvyn CLI version.");
        command.SetAction(_ =>
        {
            Console.WriteLine($"nuvyn {GetVersion()}");
            return 0;
        });
        return command;
    }

    internal static string GetVersion()
    {
        var assembly = typeof(VersionCommand).Assembly;
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "0.0.0";
    }
}
