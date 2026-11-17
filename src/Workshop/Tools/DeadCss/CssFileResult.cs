namespace Workshop.Tools.DeadCss;

internal record CssFileResult(
    string RelativePath,
    int AliveCount,
    IReadOnlyList<string> DeadClasses,
    IReadOnlyList<DynamicCssClass> DynamicClasses);
