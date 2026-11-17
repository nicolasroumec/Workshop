using System.Text.RegularExpressions;

namespace Workshop.Tools.DeadCss;

internal static partial class DeadCssAnalyzer
{
    private static readonly HashSet<string> ExcludedFolders =
        new(["bin", "obj", "node_modules", "lib", ".git", ".vs"], StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> UsageExtensions =
        new([".razor", ".cs", ".cshtml", ".html", ".js", ".ts"], StringComparer.OrdinalIgnoreCase);

    private static readonly string[] RuntimeClasses =
    [
        "valid", "invalid", "modified", "validation-message", "validation-errors", "blazor-error-boundary", "active",
        "show", "showing", "hiding", "fade", "collapsing", "modal-open", "modal-backdrop", "was-validated"
    ];

    public static async Task<IReadOnlyList<CssFileResult>> AnalyzeAsync(string rootPath, CancellationToken cancellationToken)
    {
        var files = EnumerateFiles(rootPath)
            .Where(f => !f.Contains(".min.", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var sources = new List<string>();
        foreach (var file in files.Where(f => UsageExtensions.Contains(Path.GetExtension(f))))
        {
            sources.Add(await File.ReadAllTextAsync(file, cancellationToken));
        }

        var index = CssUsageIndex.Build(sources, RuntimeClasses);

        var results = new List<CssFileResult>();
        foreach (var cssFile in files.Where(IsGlobalCss).Order(StringComparer.OrdinalIgnoreCase))
        {
            var classes = ExtractClasses(await File.ReadAllTextAsync(cssFile, cancellationToken));
            if (classes.Count > 0)
            {
                results.Add(Classify(Path.GetRelativePath(rootPath, cssFile), classes, index));
            }
        }

        return results;
    }

    public static IReadOnlyList<string> ExtractClasses(string css)
    {
        css = CommentRegex().Replace(css, "");
        css = UrlOrStringRegex().Replace(css, "");

        return SelectorRegex().Matches(css)
            .SelectMany(selector => ClassSelectorRegex().Matches(selector.Groups[1].Value))
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static CssFileResult Classify(string relativePath, IReadOnlyList<string> classes, CssUsageIndex index)
    {
        var aliveCount = 0;
        var dead = new List<string>();
        var dynamic = new List<DynamicCssClass>();

        foreach (var cssClass in classes)
        {
            switch (index.Classify(cssClass))
            {
                case CssClassStatus.Alive:
                    aliveCount++;
                    break;

                case CssClassStatus.Dead:
                    dead.Add(cssClass);
                    break;

                case CssClassStatus.Dynamic:
                    dynamic.Add(new DynamicCssClass(cssClass, index.FindPrefix(cssClass)!));
                    break;
            }
        }

        return new CssFileResult(relativePath, aliveCount, dead, dynamic);
    }

    private static bool IsGlobalCss(string path) =>
        path.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
        && !path.EndsWith(".razor.css", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> EnumerateFiles(string folder)
    {
        foreach (var file in Directory.EnumerateFiles(folder))
        {
            yield return file;
        }

        foreach (var subfolder in Directory.EnumerateDirectories(folder))
        {
            if (ExcludedFolders.Contains(Path.GetFileName(subfolder)))
            {
                continue;
            }

            foreach (var file in EnumerateFiles(subfolder))
            {
                yield return file;
            }
        }
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"url\([^)]*\)|""[^""]*""|'[^']*'")]
    private static partial Regex UrlOrStringRegex();

    [GeneratedRegex(@"([^{}]+)\{")]
    private static partial Regex SelectorRegex();

    [GeneratedRegex(@"\.(-?[A-Za-z_][\w-]*)")]
    private static partial Regex ClassSelectorRegex();
}
