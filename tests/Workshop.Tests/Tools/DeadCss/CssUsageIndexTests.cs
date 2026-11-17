using Workshop.Tools.DeadCss;

namespace Workshop.Tests.Tools.DeadCss;

public class CssUsageIndexTests
{
    private static readonly CssUsageIndex Index = CssUsageIndex.Build(
        [
            """<div class="toast-container a"><span class="@(isActive ? "btn-active" : "")"></span></div>""",
            """
            var html = "<span class=\"text-corp-success\">";
            var badge = $"badge-status-{code}";
            var level = $"fill--n{count}";
            var tone = "tone-" + color;
            """
        ],
        alwaysAlive: ["valid"]);

    [Theory]
    [InlineData("toast-container")]
    [InlineData("btn-active")]
    [InlineData("text-corp-success")]
    [InlineData("valid")]
    public void Classify_UsedClass_IsAlive(string cssClass)
    {
        Assert.Equal(CssClassStatus.Alive, Index.Classify(cssClass));
    }

    [Theory]
    [InlineData("container")]
    [InlineData("unused")]
    public void Classify_UnusedClass_IsDead(string cssClass)
    {
        Assert.Equal(CssClassStatus.Dead, Index.Classify(cssClass));
    }

    [Theory]
    [InlineData("badge-status-ok")]
    [InlineData("fill--n3")]
    [InlineData("tone-primary")]
    public void Classify_InterpolatedClass_IsDynamic(string cssClass)
    {
        Assert.Equal(CssClassStatus.Dynamic, Index.Classify(cssClass));
    }

    [Fact]
    public void FindPrefix_ReturnsLongestMatchingPrefix()
    {
        Assert.Equal("badge-status-", Index.FindPrefix("badge-status-ok"));
    }
}
