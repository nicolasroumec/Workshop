using Workshop.Tools.DeadCss;

namespace Workshop.Tests.Tools.DeadCss;

public class DeadCssAnalyzerTests
{
    [Fact]
    public void ExtractClasses_ReadsSelectorsOnly()
    {
        const string css = """
            /* .commented { } */
            .a, .b:hover > .c { margin: 1.5rem; opacity: .5; background: url(img.png); }
            @media (max-width: 768px) { .d { color: red; } }
            [data-x=".not-a-class"] .e { }
            """;

        Assert.Equal(new[] { "a", "b", "c", "d", "e" }, DeadCssAnalyzer.ExtractClasses(css));
    }

    [Fact]
    public async Task AnalyzeAsync_ReportsDeadClassesPerGlobalCssFile()
    {
        var root = Directory.CreateTempSubdirectory("workshop-tests-");
        try
        {
            var ct = TestContext.Current.CancellationToken;
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "site.css"), ".used { } .unused { }", ct);
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "Page.razor.css"), ".scoped-unused { }", ct);
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "Page.razor"), """<div class="used"></div>""", ct);

            var results = await DeadCssAnalyzer.AnalyzeAsync(root.FullName, ct);

            var file = Assert.Single(results);
            Assert.Equal("site.css", file.RelativePath);
            Assert.Equal(1, file.AliveCount);
            Assert.Equal(new[] { "unused" }, file.DeadClasses);
            Assert.Empty(file.DynamicClasses);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
