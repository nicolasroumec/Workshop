using Workshop.Tools.DuplicateCode;

namespace Workshop.Tests.Tools.DuplicateCode;

public class JscpdReportTests
{
    [Fact]
    public void Parse_ReadsTotalsAndDuplicates()
    {
        const string json = """
            {
              "duplicates": [
                {
                  "format": "css",
                  "lines": 10,
                  "tokens": 72,
                  "kind": "exact",
                  "firstFile": { "name": "Components\\A.razor.css", "start": 25, "end": 34 },
                  "secondFile": { "name": "Components\\B.razor.css", "start": 29, "end": 38 }
                }
              ],
              "statistics": {
                "total": {
                  "sources": 1077,
                  "lines": 120729,
                  "clones": 460,
                  "duplicatedLines": 8874,
                  "percentage": 7.35
                }
              }
            }
            """;

        var report = JscpdReport.Parse(json);

        Assert.Equal(460, report.Statistics.Total.Clones);
        Assert.Equal(8874, report.Statistics.Total.DuplicatedLines);
        var duplicate = Assert.Single(report.Duplicates);
        Assert.Equal("css", duplicate.Format);
        Assert.Equal(10, duplicate.Lines);
        Assert.Equal(@"Components\A.razor.css", duplicate.FirstFile.Name);
        Assert.Equal(29, duplicate.SecondFile.Start);
    }
}
