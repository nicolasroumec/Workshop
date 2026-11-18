using System.Globalization;

namespace Workshop.Tools.Clarion;

internal static class ClarionConverter
{
    private static readonly DateTime BaseDate = new(1800, 12, 28);
    private static readonly int MaxDate = (DateTime.MaxValue.Date - BaseDate).Days;

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

    private static bool TryParseClarion(string text, out int value) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
}
