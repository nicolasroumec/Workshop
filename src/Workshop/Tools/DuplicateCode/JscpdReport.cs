using System.Text.Json;

namespace Workshop.Tools.DuplicateCode;

internal record JscpdReport(IReadOnlyList<JscpdReport.Duplicate> Duplicates, JscpdReport.Summary Statistics)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static JscpdReport Parse(string json) =>
        JsonSerializer.Deserialize<JscpdReport>(json, JsonOptions)
        ?? throw new InvalidDataException("El reporte de jscpd está vacío.");

    internal record Summary(Totals Total);

    internal record Totals(int Sources, int Lines, int Clones, int DuplicatedLines, double Percentage);

    internal record Duplicate(string Format, int Lines, int Tokens, Location FirstFile, Location SecondFile);

    internal record Location(string Name, int Start, int End);
}
