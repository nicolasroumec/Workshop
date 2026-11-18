using System.Text.RegularExpressions;

namespace Workshop.Tools.Tagging;

internal static partial class TagVersion
{
    public static readonly Version First = new(0, 1, 0);

    public static Version? Latest(IEnumerable<string> tags) => tags.Select(Parse).Max();

    public static Version Next(Version current, VersionBump bump) => bump switch
    {
        VersionBump.Patch => new Version(current.Major, current.Minor, current.Build + 1),
        VersionBump.Minor => new Version(current.Major, current.Minor + 1, 0),
        VersionBump.Major => new Version(current.Major + 1, 0, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(bump)),
    };

    public static string ToTag(Version version) => $"v{version}";

    private static Version? Parse(string tag)
    {
        var match = TagRegex().Match(tag);
        return match.Success && Version.TryParse(match.Groups[1].Value, out var version) ? version : null;
    }

    [GeneratedRegex(@"\Av((?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*))\z")]
    private static partial Regex TagRegex();
}
