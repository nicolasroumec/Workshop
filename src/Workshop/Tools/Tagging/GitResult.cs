namespace Workshop.Tools.Tagging;

internal record GitResult(int ExitCode, string Output, string Error)
{
    public bool Succeeded => ExitCode == 0;

    public string[] Lines =>
        Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
