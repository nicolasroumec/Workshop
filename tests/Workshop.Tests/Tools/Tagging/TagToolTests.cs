using Spectre.Console;
using Spectre.Console.Rendering;
using Workshop.Projects;
using Workshop.Tools.Tagging;
using TempRepo = Workshop.Tests.Tools.Tagging.GitTagsTests.TempRepo;

namespace Workshop.Tests.Tools.Tagging;

public class TagToolTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Run_Confirmed_PublishesChosenVersionWithMessage()
    {
        using var repo = await RepoWithNewCommitAsync();
        var console = new ScriptedConsole();
        console.Press(ConsoleKey.DownArrow).Press(ConsoleKey.Enter).Type("Filtros [nuevos]").Press(ConsoleKey.Enter).Type("y").Press(ConsoleKey.Enter);

        await new TagTool(console).RunAsync(new Project("test", repo.Work), Token);

        Assert.Contains("Tag publicado", console.Output);
        var tags = await GitTags.RunAsync(repo.Remote, Token, "tag", "--list", "--format=%(refname:short) %(contents:subject)");
        Assert.Contains("v0.2.0 Filtros [nuevos]", tags.Lines);
    }

    [Fact]
    public async Task Run_NotConfirmed_CreatesNothing()
    {
        using var repo = await RepoWithNewCommitAsync();
        var console = new ScriptedConsole();
        console.Press(ConsoleKey.Enter).Press(ConsoleKey.Enter).Press(ConsoleKey.Enter);

        await new TagTool(console).RunAsync(new Project("test", repo.Work), Token);

        Assert.Contains("No se creó ningún tag", console.Output);
        var local = await GitTags.RunAsync(repo.Work, Token, "tag", "--list");
        var remote = await GitTags.RunAsync(repo.Remote, Token, "tag", "--list");
        Assert.Equal(["v0.1.0"], local.Lines);
        Assert.Equal(["v0.1.0"], remote.Lines);
    }

    [Fact]
    public async Task Run_RepositoryNotReady_ShowsProblemWithoutPrompting()
    {
        using var repo = await RepoWithNewCommitAsync();
        File.WriteAllText(Path.Combine(repo.Work, "new.txt"), "x");
        var console = new ScriptedConsole();

        await new TagTool(console).RunAsync(new Project("test", repo.Work), Token);

        Assert.Contains("Hay cambios sin commitear.", console.Output);
    }

    private static async Task<TempRepo> RepoWithNewCommitAsync()
    {
        var repo = await TempRepo.CreateAsync();
        await repo.CommitAsync("first");
        await repo.GitAsync("tag", "-a", "v0.1.0", "-m", "v0.1.0");
        await repo.CommitAsync("feat: [algo] nuevo");
        await repo.GitAsync("push", "origin", "master", "v0.1.0");
        return repo;
    }

    internal sealed class ScriptedConsole : IAnsiConsole, IAnsiConsoleInput
    {
        private readonly Queue<ConsoleKeyInfo> _keys = new();
        private readonly StringWriter _output = new();
        private readonly IAnsiConsole _inner;

        public ScriptedConsole()
        {
            _inner = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Yes,
                Interactive = InteractionSupport.Yes,
                Out = new AnsiConsoleOutput(_output),
            });
        }

        public string Output => _output.ToString();

        public Profile Profile => _inner.Profile;

        public IAnsiConsoleCursor Cursor => _inner.Cursor;

        public IAnsiConsoleInput Input => this;

        public IExclusivityMode ExclusivityMode => _inner.ExclusivityMode;

        public RenderPipeline Pipeline => _inner.Pipeline;

        public ScriptedConsole Press(ConsoleKey key)
        {
            _keys.Enqueue(new ConsoleKeyInfo(key == ConsoleKey.Enter ? '\r' : '\0', key, false, false, false));
            return this;
        }

        public ScriptedConsole Type(string text)
        {
            foreach (var character in text)
            {
                _keys.Enqueue(new ConsoleKeyInfo(character, ConsoleKey.NoName, false, false, false));
            }

            return this;
        }

        public void Clear(bool home) => _inner.Clear(home);

        public void Write(IRenderable renderable) => _inner.Write(renderable);

        public void WriteAnsi(Action<AnsiWriter> action) => _inner.WriteAnsi(action);

        public bool IsKeyAvailable() => _keys.Count > 0;

        public ConsoleKeyInfo? ReadKey(bool intercept) => _keys.Dequeue();

        public Task<ConsoleKeyInfo?> ReadKeyAsync(bool intercept, CancellationToken cancellationToken) =>
            Task.FromResult<ConsoleKeyInfo?>(_keys.Dequeue());
    }
}
