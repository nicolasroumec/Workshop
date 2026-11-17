using Spectre.Console;

namespace Workshop;

internal static class Theme
{
    public const string Accent = "deepskyblue1";
    public const string Muted = "grey";
    public const string Success = "green";
    public const string Warning = "yellow";
    public const string Danger = "red";

    public static string FileLink(string fullPath, string text) =>
        $"[link={new Uri(fullPath).AbsoluteUri}]{Markup.Escape(text)}[/]";
}
