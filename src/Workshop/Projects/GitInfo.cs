using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Workshop.Projects;

internal record GitInfo(string Branch, int? Ahead)
{
    private const string BranchPrefix = "# branch.head ";
    private const string AheadPrefix = "# branch.ab +";

    public static GitInfo? Read(string path)
    {
        var startInfo = new ProcessStartInfo("git", ["-C", path, "status", "--porcelain=v2", "--branch", "--untracked-files=no"])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode == 0 ? Parse(output) : null;
        }
        catch (Win32Exception)
        {
            return null;
        }
    }

    public static GitInfo? Parse(string output)
    {
        string? branch = null;
        int? ahead = null;

        foreach (var line in output.Split('\n', StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith(BranchPrefix, StringComparison.Ordinal))
            {
                branch = line[BranchPrefix.Length..];
            }
            else if (line.StartsWith(AheadPrefix, StringComparison.Ordinal))
            {
                ahead = int.Parse(line[AheadPrefix.Length..].Split(' ')[0], CultureInfo.InvariantCulture);
            }
        }

        return branch is null ? null : new GitInfo(branch, ahead);
    }
}
