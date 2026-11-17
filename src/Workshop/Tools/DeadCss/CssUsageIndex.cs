using System.Text.RegularExpressions;

namespace Workshop.Tools.DeadCss;

internal sealed partial class CssUsageIndex
{
    private readonly HashSet<string> _tokens;
    private readonly HashSet<string> _prefixes;

    private CssUsageIndex(HashSet<string> tokens, HashSet<string> prefixes)
    {
        _tokens = tokens;
        _prefixes = prefixes;
    }

    public static CssUsageIndex Build(IEnumerable<string> sources, IEnumerable<string> alwaysAlive)
    {
        var tokens = new HashSet<string>(alwaysAlive, StringComparer.Ordinal);
        var prefixes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var source in sources)
        {
            foreach (Match match in TokenRegex().Matches(source))
            {
                tokens.Add(match.Value);
            }

            foreach (Match match in InterpolatedPrefixRegex().Matches(source))
            {
                prefixes.Add(match.Value);
            }
        }

        prefixes.UnionWith(tokens.Where(t => t.EndsWith('-')));
        prefixes.RemoveWhere(p => p.Length <= 2);

        return new CssUsageIndex(tokens, prefixes);
    }

    public CssClassStatus Classify(string cssClass)
    {
        if (_tokens.Contains(cssClass))
        {
            return CssClassStatus.Alive;
        }

        return FindPrefix(cssClass) is null ? CssClassStatus.Dead : CssClassStatus.Dynamic;
    }

    public string? FindPrefix(string cssClass) =>
        _prefixes
            .Where(p => cssClass.StartsWith(p, StringComparison.Ordinal))
            .MaxBy(p => p.Length);

    [GeneratedRegex(@"[A-Za-z_][\w-]*")]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"[A-Za-z_][\w-]*(?=\{|\$\{|@|[""']\s*\+)")]
    private static partial Regex InterpolatedPrefixRegex();
}
