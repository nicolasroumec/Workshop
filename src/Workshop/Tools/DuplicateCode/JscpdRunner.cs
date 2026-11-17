using System.Diagnostics;

namespace Workshop.Tools.DuplicateCode;

internal static class JscpdRunner
{
    private const string IgnoredPaths =
        "**/bin/**,**/obj/**,**/node_modules/**,**/wwwroot/lib/**,**/Migrations/**,**/*.min.*";

    public static async Task<JscpdRunResult> RunAsync(
        string projectPath, int minTokens, string outputFolder, CancellationToken cancellationToken)
    {
        var arguments = string.Join(' ',
            "/c npx --yes jscpd .",
            $"--min-tokens {minTokens}",
            "--format \"csharp,markup,css\"",
            "--formats-exts \"csharp:cs;markup:razor,html;css:css\"",
            $"--ignore \"{IgnoredPaths}\"",
            "--reporters json,html",
            "--silent",
            $"--output \"{outputFolder}\"");

        using var process = Process.Start(new ProcessStartInfo("cmd.exe", arguments)
        {
            WorkingDirectory = projectPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }) ?? throw new InvalidOperationException("No se pudo iniciar jscpd.");

        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        return new JscpdRunResult(process.ExitCode, await output, await error);
    }
}
