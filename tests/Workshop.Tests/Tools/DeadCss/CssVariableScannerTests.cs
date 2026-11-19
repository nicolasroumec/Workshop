using Workshop.Tools.DeadCss;

namespace Workshop.Tests.Tools.DeadCss;

public class CssVariableScannerTests
{
    private static CssVariableReport Scan(params (string Path, string Text)[] files)
    {
        var scanner = new CssVariableScanner();
        foreach (var (path, text) in files)
        {
            scanner.Scan(path, text);
        }

        return scanner.Report();
    }

    private static string[] Names(IReadOnlyList<CssVariableIssue> issues) => issues.Select(i => i.Name).ToArray();

    [Fact]
    public void DeclarationsInCss_ScopedCss_AndInlineStyles_CountAsDeclared()
    {
        var report = Scan(
            ("site.css", ":root { --global: 1; }"),
            ("Page.razor.css", ".a { --scoped: 1; }"),
            ("Page.razor", """<div style="--progreso: @valor%"></div>"""),
            ("uses.css", ".b { width: var(--global); height: var(--scoped); left: var(--progreso); }"));

        Assert.Empty(report.Undeclared);
        Assert.Empty(report.Unused);
    }

    [Fact]
    public void UndeclaredUses_AreSplitByFallback()
    {
        var report = Scan(("site.css", ".a { color: var(--sin); background: var( --con , red); }"));

        Assert.Equal(new[] { "--sin" }, Names(report.Undeclared));
        Assert.Equal(new[] { "--con" }, Names(report.UndeclaredWithFallback));
    }

    [Fact]
    public void DeclaredButNeverUsed_IsUnused()
    {
        var report = Scan(("site.css", ":root { --usada: 1; --huerfana: 2; } .a { color: var(--usada); }"));

        Assert.Equal(new[] { "--huerfana" }, Names(report.Unused));
    }

    [Fact]
    public void ExternalPrefix_CountsAsDeclared_AndIsNeverUnused()
    {
        var report = Scan(("site.css", ":root { --bs-primary: red; } .a { color: var(--bs-body-color); }"));

        Assert.Empty(report.Undeclared);
        Assert.Empty(report.Unused);
    }

    [Fact]
    public void JsSetPropertyAndGetPropertyValue_CountAsUse()
    {
        var report = Scan(
            ("site.css", ":root { --leida: 1; --escrita: 1; } .a { width: var(--seteada); }"),
            ("app.js", "el.style.setProperty('--seteada', '1px'); getComputedStyle(el).getPropertyValue(\"--leida\"); el.style.setProperty(`--escrita`, 2);"));

        Assert.Empty(report.Undeclared);
        Assert.Empty(report.Unused);
    }

    [Fact]
    public void NestedVar_FallbackVariableIsAlsoAUse()
    {
        var report = Scan(("site.css", ":root { --b: 1; } .a { color: var(--a, var(--b)); width: var(--c, var(--d)); }"));

        Assert.Equal(new[] { "--a", "--c" }, Names(report.UndeclaredWithFallback));
        Assert.Equal(new[] { "--d" }, Names(report.Undeclared));
        Assert.Empty(report.Unused);
    }

    [Fact]
    public void DynamicRazorNames_AreIgnored()
    {
        var report = Scan(("Page.razor", """<div style="--@nombre: 1; --color-@tono: 2; width: var(--@nombre); color: var(--color-@tono)"></div>"""));

        Assert.Empty(report.Undeclared);
        Assert.Empty(report.Unused);
    }

    [Fact]
    public void VarInsideComments_IsNotAUse()
    {
        var report = Scan(("site.css", ":root { --x: 1; } /* .a { color: var(--x); var(--fantasma); } */"));

        Assert.Equal(new[] { "--x" }, Names(report.Unused));
        Assert.Empty(report.Undeclared);
    }

    [Fact]
    public void Locations_HaveFileAndLineNumber()
    {
        var path = Path.Combine("Components", "Ficha.razor.css");
        var report = Scan((path, "/* uno\n dos */\n.a {\n  color: var(--corp-gris);\n}\n.b { color: var(--corp-gris); }"));

        var issue = Assert.Single(report.Undeclared);
        Assert.Equal(
            new[] { new CssVariableLocation(path, 4), new CssVariableLocation(path, 6) },
            issue.Locations);
    }
}
