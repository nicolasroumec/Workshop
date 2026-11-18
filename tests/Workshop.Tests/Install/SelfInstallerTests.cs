using System.Runtime.Versioning;
using Microsoft.Win32;
using Workshop.Install;

namespace Workshop.Tests.Install;

[SupportedOSPlatform("windows")]
public class SelfInstallerTests
{
    private const string Folder = @"C:\Tools\Workshop";

    [Fact]
    public void FindRepository_WalksUpToTheSolution()
    {
        var root = Directory.CreateTempSubdirectory("workshop-tests-").FullName;
        try
        {
            File.WriteAllText(Path.Combine(root, "Workshop.slnx"), "");
            var nested = Directory.CreateDirectory(Path.Combine(root, "src", "bin")).FullName;

            Assert.Equal(root, SelfInstaller.FindRepository(nested));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void FindRepository_NoSolution_ReturnsNull()
    {
        var root = Directory.CreateTempSubdirectory("workshop-tests-").FullName;
        try
        {
            Assert.Null(SelfInstaller.FindRepository(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PublishArguments_BuildsSingleFileForTheRuntime()
    {
        var arguments = SelfInstaller.PublishArguments(@"C:\repo", Folder, "win-arm64");

        Assert.Equal(@"C:\repo\src\Workshop\Workshop.csproj", arguments[1]);
        Assert.Equal("win-arm64", arguments[Array.IndexOf(arguments, "-r") + 1]);
        Assert.Equal(Folder, arguments[Array.IndexOf(arguments, "-o") + 1]);
        Assert.Contains("-p:PublishSingleFile=true", arguments);
    }

    [Theory]
    [InlineData(null, Folder)]
    [InlineData("", Folder)]
    [InlineData(@"C:\A", @"C:\A;C:\Tools\Workshop")]
    [InlineData(@"C:\A;", @"C:\A;C:\Tools\Workshop")]
    [InlineData(@"%USERPROFILE%\bin", @"%USERPROFILE%\bin;C:\Tools\Workshop")]
    public void WithFolder_MissingFolder_AppendsIt(string? path, string expected)
    {
        Assert.Equal(expected, SelfInstaller.WithFolder(path, Folder));
    }

    [Theory]
    [InlineData(@"C:\Tools\Workshop")]
    [InlineData(@"C:\A;c:\tools\workshop")]
    [InlineData(@"C:\Tools\Workshop\;C:\A")]
    [InlineData(@"""C:\Tools\Workshop"";C:\A")]
    public void WithFolder_FolderPresent_ReturnsNull(string path)
    {
        Assert.Null(SelfInstaller.WithFolder(path, Folder));
    }

    [Fact]
    public void AddToPath_KeepsVariablesAndDoesNotDuplicate()
    {
        var keyName = @"Software\WorkshopTests-" + Guid.NewGuid().ToString("N");
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(keyName);
            key.SetValue("Path", @"%USERPROFILE%\bin", RegistryValueKind.ExpandString);

            Assert.True(SelfInstaller.AddToPath(key, Folder));
            Assert.False(SelfInstaller.AddToPath(key, Folder));

            Assert.Equal(
                @"%USERPROFILE%\bin;C:\Tools\Workshop",
                key.GetValue("Path", null, RegistryValueOptions.DoNotExpandEnvironmentNames));
            Assert.Equal(RegistryValueKind.ExpandString, key.GetValueKind("Path"));
        }
        finally
        {
            Registry.CurrentUser.DeleteSubKeyTree(keyName, throwOnMissingSubKey: false);
        }
    }
}
