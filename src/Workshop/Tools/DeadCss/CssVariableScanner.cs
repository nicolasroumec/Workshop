using System.Text.RegularExpressions;

namespace Workshop.Tools.DeadCss;

internal record CssVariableLocation(string RelativePath, int Line);

internal record CssVariableIssue(string Name, IReadOnlyList<CssVariableLocation> Locations);

internal record CssVariableReport(
    IReadOnlyList<CssVariableIssue> Undeclared,
    IReadOnlyList<CssVariableIssue> UndeclaredWithFallback,
    IReadOnlyList<CssVariableIssue> Unused);

internal sealed partial class CssVariableScanner
{
    // Variables de librerías que se cargan desde fuera del repo (CDN): cuentan como declaradas y nunca como sin uso.
    private static readonly string[] ExternalPrefixes = ["--bs-"];

    private readonly Dictionary<string, List<CssVariableLocation>> _declarations = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<CssVariableLocation>> _uses = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<CssVariableLocation>> _usesWithFallback = new(StringComparer.Ordinal);
    private readonly HashSet<string> _touchedFromJs = new(StringComparer.Ordinal);

    public void Scan(string relativePath, string text)
    {
        // Blanquea los comentarios conservando los saltos de línea para no correr los números de línea.
        text = CommentRegex().Replace(text, m => NotNewLineRegex().Replace(m.Value, " "));

        foreach (Match match in DeclarationRegex().Matches(text))
        {
            Add(_declarations, match.Groups[1].Value, Location(relativePath, text, match.Index));
        }

        foreach (Match match in UseRegex().Matches(text))
        {
            Add(match.Groups[2].Success ? _usesWithFallback : _uses, match.Groups[1].Value, Location(relativePath, text, match.Index));
        }

        foreach (Match match in JsPropertyRegex().Matches(text))
        {
            _touchedFromJs.Add(match.Groups[1].Value);
        }
    }

    public CssVariableReport Report()
    {
        // setProperty también declara la variable, así que lo tocado desde JS cuenta como declarado y como usado.
        bool IsDeclared(string name) => _declarations.ContainsKey(name) || _touchedFromJs.Contains(name) || IsExternal(name);
        bool IsUsed(string name) => _uses.ContainsKey(name) || _usesWithFallback.ContainsKey(name) || _touchedFromJs.Contains(name);

        return new CssVariableReport(
            Issues(_uses, name => !IsDeclared(name)),
            Issues(_usesWithFallback, name => !IsDeclared(name)),
            Issues(_declarations, name => !IsUsed(name) && !IsExternal(name)));
    }

    private static bool IsExternal(string name) =>
        ExternalPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal));

    private static List<CssVariableIssue> Issues(Dictionary<string, List<CssVariableLocation>> source, Func<string, bool> predicate) =>
        source
            .Where(entry => predicate(entry.Key))
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new CssVariableIssue(entry.Key, entry.Value))
            .ToList();

    private static void Add(Dictionary<string, List<CssVariableLocation>> target, string name, CssVariableLocation location)
    {
        if (!target.TryGetValue(name, out var locations))
        {
            target[name] = locations = [];
        }

        locations.Add(location);
    }

    // ponytail: recuenta saltos de línea por cada coincidencia, O(n·m); precalcular inicios de línea si algún archivo enorme lo pide.
    private static CssVariableLocation Location(string relativePath, string text, int index) =>
        new(relativePath, text.AsSpan(0, index).Count('\n') + 1);

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"[^\n]")]
    private static partial Regex NotNewLineRegex();

    // --x: valor. Los nombres dinámicos de Razor (--@nombre) no matchean porque @ no es parte del nombre.
    [GeneratedRegex(@"(?<![\w-])(--[\w-]+)\s*:")]
    private static partial Regex DeclarationRegex();

    // var(--x) o var(--x, respaldo). El lookahead descarta nombres que siguen con @, { o ${ (armados en Razor, C# o JS).
    [GeneratedRegex(@"var\(\s*(--[\w-]+)(?![\w@{$-])\s*(,)?")]
    private static partial Regex UseRegex();

    [GeneratedRegex(@"(?:setProperty|getPropertyValue)\(\s*['""`](--[\w-]+)['""`]")]
    private static partial Regex JsPropertyRegex();
}
