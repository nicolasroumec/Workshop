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

            var progress = new List<(int Done, int Total)>();
            var audit = await DeadCssAnalyzer.AnalyzeAsync(root.FullName, (done, total) => progress.Add((done, total)), ct);

            Assert.Equal(new[] { (1, 3), (2, 3), (3, 3) }, progress);
            var file = Assert.Single(audit.Files);
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

    [Fact]
    public async Task AnalyzeAsync_ScansVariablesInScopedCssAndUsageFiles()
    {
        var root = Directory.CreateTempSubdirectory("workshop-tests-");
        try
        {
            var ct = TestContext.Current.CancellationToken;
            Directory.CreateDirectory(Path.Combine(root.FullName, "obj"));
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "obj", "generated.css"), ".x { --ignorada: 1; }", ct);
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "Page.razor.css"), ".a { color: var(--color); }", ct);
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "Page.razor"), """<div style="--color: red"></div>""", ct);

            var audit = await DeadCssAnalyzer.AnalyzeAsync(root.FullName, null, ct);

            Assert.Empty(audit.Variables.Undeclared);
            Assert.Empty(audit.Variables.Unused);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
