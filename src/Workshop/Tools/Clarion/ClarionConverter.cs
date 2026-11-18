using System.Globalization;

namespace Workshop.Tools.Clarion;

internal static class ClarionConverter
{
    private const int MaxTime = 8640000;
    private const long TicksPerHundredth = TimeSpan.TicksPerMillisecond * 10;

    private static readonly DateTime BaseDate = new(1800, 12, 28);
    private static readonly int MaxDate = (DateTime.MaxValue.Date - BaseDate).Days;
    private static readonly string[] TimeFormats = [@"h\:mm", @"h\:mm\:ss", @"h\:mm\:ss\.FF"];

    public static string? ConvertDate(string text)
    {
        if (TryParseClarion(text, out var clarion))
        {
            return clarion >= 1 && clarion <= MaxDate
                ? BaseDate.AddDays(clarion).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                : null;
        }

        return DateTime.TryParseExact(text, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) && date > BaseDate
            ? (date - BaseDate).Days.ToString(CultureInfo.InvariantCulture)
            : null;
    }

    public static string? ConvertTime(string text)
    {
        if (TryParseClarion(text, out var clarion))
        {
            return clarion >= 1 && clarion <= MaxTime
                ? TimeSpan.FromTicks((clarion - 1) * TicksPerHundredth).ToString(@"hh\:mm\:ss\.ff", CultureInfo.InvariantCulture)
                : null;
        }

        return TimeSpan.TryParseExact(text, TimeFormats, CultureInfo.InvariantCulture, out var time)
            ? (time.Ticks / TicksPerHundredth + 1).ToString(CultureInfo.InvariantCulture)
            : null;
    }

    public static string? ConvertDateTime(string text)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return null;
        }

        var date = ConvertDate(parts[0]);
        var time = ConvertTime(parts[1]);
        return date is null || time is null ? null : $"{date} {time}";
    }

    private static bool TryParseClarion(string text, out int value) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
}
