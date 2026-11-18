using Workshop.Tools.Tagging;

namespace Workshop.Tests.Tools.Tagging;

public class TagVersionTests
{
    [Fact]
    public void Latest_NoTags_ReturnsNull()
    {
        Assert.Null(TagVersion.Latest([]));
    }

    [Fact]
    public void Latest_ComparesNumbersNotText()
    {
        Assert.Equal(new Version(0, 10, 0), TagVersion.Latest(["v0.9.0", "v0.10.0", "v0.2.5"]));
    }

    [Theory]
    [InlineData("0.9.0")]
    [InlineData("v1.2")]
    [InlineData("v1.2.3.4")]
    [InlineData("v1.2.3-beta")]
    [InlineData("v01.2.3")]
    [InlineData("v+1.2.3")]
    [InlineData("v1.2.3\n")]
    [InlineData("v99999999999.0.0")]
    [InlineData("release")]
    public void Latest_IgnoresOtherFormats(string tag)
    {
        Assert.Equal(new Version(0, 1, 0), TagVersion.Latest([tag, "v0.1.0"]));
    }

    [Fact]
    public void Next_Patch_BumpsPatch()
    {
        Assert.Equal("v1.2.4", TagVersion.ToTag(TagVersion.Next(new Version(1, 2, 3), VersionBump.Patch)));
    }

    [Fact]
    public void Next_Minor_ResetsPatch()
    {
        Assert.Equal("v1.3.0", TagVersion.ToTag(TagVersion.Next(new Version(1, 2, 3), VersionBump.Minor)));
    }

    [Fact]
    public void Next_Major_ResetsMinorAndPatch()
    {
        Assert.Equal("v2.0.0", TagVersion.ToTag(TagVersion.Next(new Version(1, 2, 3), VersionBump.Major)));
    }

    [Fact]
    public void First_IsTheInitialTag()
    {
        Assert.Equal("v0.1.0", TagVersion.ToTag(TagVersion.First));
    }
}
