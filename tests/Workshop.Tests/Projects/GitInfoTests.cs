using Workshop.Projects;

namespace Workshop.Tests.Projects;

public class GitInfoTests
{
    [Fact]
    public void Parse_BranchWithUpstream_ReadsBranchAndAhead()
    {
        var output = "# branch.oid abc123\n# branch.head feature/visual-refresh\n# branch.upstream origin/feature/visual-refresh\n# branch.ab +3 -1\n";

        Assert.Equal(new GitInfo("feature/visual-refresh", 3), GitInfo.Parse(output));
    }

    [Fact]
    public void Parse_BranchWithoutUpstream_HasNoAhead()
    {
        var output = "# branch.oid abc123\n# branch.head master\n1 .M N... 100644 100644 100644 a b src/Program.cs\n";

        Assert.Equal(new GitInfo("master", null), GitInfo.Parse(output));
    }

    [Fact]
    public void Parse_EmptyOutput_ReturnsNull()
    {
        Assert.Null(GitInfo.Parse(""));
    }
}
