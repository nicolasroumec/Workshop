using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Win32;

namespace Workshop.Install;

[SupportedOSPlatform("windows")]
internal static class SelfInstaller
{
    private const string SolutionFile = "Workshop.slnx";
    private const string PathValue = "Path";
    private const string RefreshVariable = "WORKSHOP_PATH_REFRESH";

    public static string InstallFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Workshop");

    public static string? FindRepository(string startFolder)
    {
        for (var folder = new DirectoryInfo(startFolder); folder is not null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, SolutionFile)))
            {
                return folder.FullName;
            }
        }

        return null;
    }

    public static string[] PublishArguments(string repository, string output, string runtime) =>
    [
        "publish",
        Path.Combine(repository, "src", "Workshop", "Workshop.csproj"),
        "-c", "Release",
        "-r", runtime,
        "--self-contained", "false",
        "-p:PublishSingleFile=true",
        "-o", output,
        "-v", "q",
        "--nologo",
    ];

    /// <returns>The error text, or null when the publish succeeded.</returns>
    public static async Task<string?> PublishAsync(string repository, string output, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo("dotnet", PublishArguments(repository, output, RuntimeInformation.RuntimeIdentifier))
        {
            WorkingDirectory = repository,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
        };

        Process? started;
        try
        {
            started = Process.Start(startInfo);
        }
        catch (Win32Exception)
        {
            return "No se encontró dotnet.";
        }

        using var process = started ?? throw new InvalidOperationException("No se pudo iniciar dotnet.");
        var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        return process.ExitCode == 0 ? null : $"{await standardOutput}{await standardError}".Trim();
    }

    public static bool AddToUserPath(string folder)
    {
        using var environment = Registry.CurrentUser.CreateSubKey("Environment");
        if (!AddToPath(environment, folder))
        {
            return false;
        }

        // ponytail: removing a variable makes .NET broadcast WM_SETTINGCHANGE, so no P/Invoke is needed.
        Environment.SetEnvironmentVariable(RefreshVariable, null, EnvironmentVariableTarget.User);
        return true;
    }

    public static bool AddToPath(RegistryKey environment, string folder)
    {
        var current = environment.GetValue(PathValue, null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
        var updated = WithFolder(current, folder);
        if (updated is null)
        {
            return false;
        }

        var kind = current is null ? RegistryValueKind.ExpandString : environment.GetValueKind(PathValue);
        environment.SetValue(PathValue, updated, kind);
        return true;
    }

    /// <returns>The new PATH, or null when the folder is already in it.</returns>
    public static string? WithFolder(string? path, string folder)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return folder;
        }

        var target = Normalize(folder);
        return path.Split(';').Any(entry => string.Equals(Normalize(entry), target, StringComparison.OrdinalIgnoreCase))
            ? null
            : $"{path.TrimEnd(';')};{folder}";
    }

    private static string Normalize(string entry) =>
        Environment.ExpandEnvironmentVariables(entry.Trim().Trim('"')).TrimEnd('\\', '/');
}
