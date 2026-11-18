namespace Workshop.Tools.Tagging;

internal record TagPlan(string Commit, string Branch, Version? Latest, IReadOnlyList<string> Commits);
