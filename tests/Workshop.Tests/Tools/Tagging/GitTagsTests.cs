using Workshop.Tools.Tagging;

namespace Workshop.Tests.Tools.Tagging;

public class GitTagsTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public void ParseDefaultBranch_ReadsSymbolicRef()
    {
        Assert.Equal("master", GitTags.ParseDefaultBranch("ref: refs/heads/master\tHEAD\nabc123\tHEAD\n"));
    }

    [Fact]
    public void ParseDefaultBranch_NoSymbolicRef_ReturnsNull()
    {
        Assert.Null(GitTags.ParseDefaultBranch("abc123\tHEAD\n"));
    }

    [Fact]
    public async Task Prepare_FolderWithoutRepository_ReportsProblem()
    {
        using var repo = await TempRepo.CreateAsync();

        var (plan, problem) = await GitTags.PrepareAsync(repo.Outside, Token);

        Assert.Null(plan);
        Assert.StartsWith("La carpeta no es un repositorio", problem);
    }

    [Fact]
    public async Task Prepare_NoTags_ListsAllCommits()
    {
        using var repo = await TempRepo.CreateAsync();
        await repo.CommitAsync("feat: ñandú");
        await repo.GitAsync("push", "origin", "master");

        var (plan, problem) = await GitTags.PrepareAsync(repo.Work, Token);

        Assert.Null(problem);
        Assert.Equal("master", plan!.Branch);
        Assert.Null(plan.Latest);
        Assert.EndsWith("feat: ñandú", Assert.Single(plan.Commits));
    }

    [Fact]
    public async Task Prepare_AfterTag_ListsOnlyNewCommits()
    {
        using var repo = await TempRepo.CreateAsync();
        await repo.CommitAsync("first");
        await repo.GitAsync("tag", "v0.1.0");
        await repo.CommitAsync("second");
        await repo.GitAsync("push", "origin", "master", "v0.1.0");

        var (plan, _) = await GitTags.PrepareAsync(repo.Work, Token);

        Assert.Equal(new Version(0, 1, 0), plan!.Latest);
        Assert.EndsWith("second", Assert.Single(plan.Commits));
    }

    [Fact]
    public async Task Prepare_UncommittedChanges_ReportsProblem()
    {
        using var repo = await TempRepo.CreateAsync();
        await repo.CommitAsync("first");
        await repo.GitAsync("push", "origin", "master");
        File.WriteAllText(Path.Combine(repo.Work, "new.txt"), "x");

        var (_, problem) = await GitTags.PrepareAsync(repo.Work, Token);

        Assert.Equal("Hay cambios sin commitear.", problem);
    }

    [Fact]
    public async Task Prepare_UnpushedCommit_ReportsProblem()
    {
        using var repo = await TempRepo.CreateAsync();
        await repo.CommitAsync("first");
        await repo.GitAsync("push", "origin", "master");
        await repo.CommitAsync("second");

        var (_, problem) = await GitTags.PrepareAsync(repo.Work, Token);

        Assert.StartsWith("El commit actual no es el último de master", problem);
    }

    [Fact]
    public async Task Prepare_BehindRemote_ReportsProblem()
    {
        using var repo = await TempRepo.CreateAsync();
        await repo.CommitAsync("first");
        await repo.CommitAsync("second");
        await repo.GitAsync("push", "origin", "master");
        await repo.GitAsync("reset", "--hard", "HEAD~1");

        var (_, problem) = await GitTags.PrepareAsync(repo.Work, Token);

        Assert.StartsWith("El commit actual no es el último de master", problem);
    }

    [Fact]
    public async Task Prepare_NoNewCommits_ReportsProblem()
    {
        using var repo = await TempRepo.CreateAsync();
        await repo.CommitAsync("first");
        await repo.GitAsync("tag", "v0.1.0");
        await repo.GitAsync("push", "origin", "master", "v0.1.0");

        var (_, problem) = await GitTags.PrepareAsync(repo.Work, Token);

        Assert.Equal("No hay commits nuevos desde v0.1.0.", problem);
    }

    [Fact]
    public async Task Publish_PushesAnnotatedTagToCommit()
    {
        using var repo = await TempRepo.CreateAsync();
        await repo.CommitAsync("first");
        await repo.GitAsync("push", "origin", "master");
        var (plan, _) = await GitTags.PrepareAsync(repo.Work, Token);

        var result = await GitTags.PublishAsync(repo.Work, "v0.1.0", "Primera versión", plan!.Commit, Token);

        Assert.True(result.Succeeded, result.Error);
        var remote = await GitTags.RunAsync(
            repo.Remote, Token, "tag", "--list", "--format=%(objecttype) %(*objectname) %(contents:subject)");
        Assert.Equal($"tag {plan.Commit} Primera versión", Assert.Single(remote.Lines));
    }

    [Fact]
    public async Task Publish_PushFails_RemovesLocalTag()
    {
        using var repo = await TempRepo.CreateAsync();
        await repo.CommitAsync("first");
        await repo.GitAsync("push", "origin", "master");
        var (plan, _) = await GitTags.PrepareAsync(repo.Work, Token);
        await repo.GitAsync("remote", "set-url", "--push", "origin", Path.Combine(repo.Outside, "missing.git"));

        var result = await GitTags.PublishAsync(repo.Work, "v0.1.0", "Primera versión", plan!.Commit, Token);

        Assert.False(result.Succeeded);
        var tags = await GitTags.RunAsync(repo.Work, Token, "tag", "--list");
        Assert.Empty(tags.Lines);
    }

    private sealed class TempRepo : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "workshop-tests-" + Guid.NewGuid().ToString("N"));
        private int _commits;

        public string Remote => Path.Combine(_root, "remote.git");

        public string Work => Path.Combine(_root, "work");

        public string Outside => Path.Combine(_root, "outside");

        public static async Task<TempRepo> CreateAsync()
        {
            var repo = new TempRepo();
            Directory.CreateDirectory(repo.Outside);
            await Run(repo._root, "init", "--bare", "-b", "master", repo.Remote);
            await Run(repo._root, "clone", repo.Remote, repo.Work);
            await repo.GitAsync("config", "user.name", "Test");
            await repo.GitAsync("config", "user.email", "test@example.com");
            await repo.GitAsync("config", "commit.gpgsign", "false");
            await repo.GitAsync("config", "tag.gpgsign", "false");
            return repo;
        }

        public async Task CommitAsync(string message)
        {
            File.WriteAllText(Path.Combine(Work, $"file{++_commits}.txt"), message);
            await GitAsync("add", ".");
            await GitAsync("commit", "-m", message);
        }

        public Task GitAsync(params string[] arguments) => Run(Work, arguments);

        public void Dispose()
        {
            foreach (var file in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(_root, recursive: true);
        }

        private static async Task Run(string path, params string[] arguments)
        {
            Directory.CreateDirectory(path);
            var result = await GitTags.RunAsync(path, Token, arguments);
            Assert.True(result.Succeeded, result.Error);
        }
    }
}
