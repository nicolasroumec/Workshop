using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Workshop.Tools.Tagging;

internal static class GitTags
{
    private const string HeadRefPrefix = "ref: refs/heads/";
    private const string HeadRefSuffix = "\tHEAD";

    public static async Task<(TagPlan? Plan, string? Problem)> PrepareAsync(string path, CancellationToken cancellationToken)
    {
        var repository = await RunAsync(path, cancellationToken, "rev-parse", "--is-inside-work-tree");
        if (!repository.Succeeded)
        {
            return Problem("La carpeta no es un repositorio de git.", repository);
        }

        var remoteHead = await RunAsync(path, cancellationToken, "ls-remote", "--symref", "origin", "HEAD");
        if (!remoteHead.Succeeded)
        {
            return Problem("No se pudo consultar origin.", remoteHead);
        }

        var branch = ParseDefaultBranch(remoteHead.Output);
        if (branch is null)
        {
            return (null, "origin no tiene una rama principal.");
        }

        var status = await RunAsync(path, cancellationToken, "status", "--porcelain");
        if (!status.Succeeded)
        {
            return Problem("No se pudo leer el estado del repositorio.", status);
        }

        if (status.Lines.Length > 0)
        {
            return (null, "Hay cambios sin commitear.");
        }

        var remoteRef = $"refs/remotes/origin/{branch}";
        var fetch = await RunAsync(path, cancellationToken, "fetch", "--tags", "origin", $"+refs/heads/{branch}:{remoteRef}");
        if (!fetch.Succeeded)
        {
            return Problem("No se pudo actualizar desde origin.", fetch);
        }

        var head = await RunAsync(path, cancellationToken, "rev-parse", "--verify", "HEAD");
        var remote = await RunAsync(path, cancellationToken, "rev-parse", "--verify", remoteRef);
        if (!head.Succeeded || !remote.Succeeded)
        {
            return Problem("No se pudo leer el último commit.", head.Succeeded ? remote : head);
        }

        if (head.Output != remote.Output)
        {
            return (null, $"El commit actual no es el último de {branch} en origin. Cambiá a {branch} y sincronizalo.");
        }

        var tags = await RunAsync(path, cancellationToken, "tag", "--list");
        if (!tags.Succeeded)
        {
            return Problem("No se pudieron leer los tags.", tags);
        }

        var latest = TagVersion.Latest(tags.Lines);
        var range = latest is null ? "HEAD" : $"{TagVersion.ToTag(latest)}..HEAD";
        var log = await RunAsync(path, cancellationToken, "log", "--no-merges", "--format=%h %s", range);
        if (!log.Succeeded)
        {
            return Problem("No se pudieron leer los commits.", log);
        }

        if (log.Lines.Length == 0)
        {
            return (null, $"No hay commits nuevos desde {(latest is null ? "el inicio" : TagVersion.ToTag(latest))}.");
        }

        return (new TagPlan(head.Output.Trim(), branch, latest, log.Lines), null);
    }

    public static async Task<GitResult> PublishAsync(
        string path, string tag, string message, string commit, CancellationToken cancellationToken)
    {
        var created = await RunAsync(path, cancellationToken, "tag", "-a", tag, "-m", message, commit);
        if (!created.Succeeded)
        {
            return created;
        }

        try
        {
            var pushed = await RunAsync(path, cancellationToken, "push", "origin", $"refs/tags/{tag}");
            if (!pushed.Succeeded)
            {
                await RunAsync(path, CancellationToken.None, "tag", "-d", tag);
            }

            return pushed;
        }
        catch (OperationCanceledException)
        {
            await RunAsync(path, CancellationToken.None, "tag", "-d", tag);
            throw;
        }
    }

    public static string? ParseDefaultBranch(string lsRemoteOutput) =>
        lsRemoteOutput.Split('\n', StringSplitOptions.TrimEntries)
            .Where(line => line.StartsWith(HeadRefPrefix, StringComparison.Ordinal)
                && line.EndsWith(HeadRefSuffix, StringComparison.Ordinal))
            .Select(line => line[HeadRefPrefix.Length..^HeadRefSuffix.Length])
            .FirstOrDefault();

    public static async Task<GitResult> RunAsync(string path, CancellationToken cancellationToken, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("git", ["-C", path, .. arguments])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
        };
        startInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";

        Process? started;
        try
        {
            started = Process.Start(startInfo);
        }
        catch (Win32Exception)
        {
            return new GitResult(-1, "", "No se encontró git.");
        }

        using var process = started ?? throw new InvalidOperationException("No se pudo iniciar git.");
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

        return new GitResult(process.ExitCode, await output, await error);
    }

    private static (TagPlan? Plan, string? Problem) Problem(string message, GitResult result) =>
        (null, $"{message} {result.Error.Trim()}".TrimEnd());
}
