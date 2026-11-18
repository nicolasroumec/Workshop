using Workshop.Projects;
using Workshop.Tools.Clarion;
using ScriptedConsole = Workshop.Tests.Tools.Tagging.TagToolTests.ScriptedConsole;

namespace Workshop.Tests.Tools.Clarion;

public class ClarionToolTests
{
    [Fact]
    public async Task Run_ConvertsUntilEmptyInput()
    {
        var console = new ScriptedConsole();
        console.Type("81453").Press(ConsoleKey.Enter)
            .Type("2/1/2024").Press(ConsoleKey.Enter)
            .Type("abc").Press(ConsoleKey.Enter)
            .Press(ConsoleKey.Enter);

        await new ClarionTool(console).RunAsync(new Project("test", "."), TestContext.Current.CancellationToken);

        Assert.Contains("01/01/2024", console.Output);
        Assert.Contains("81454", console.Output);
        Assert.Contains("Valor no válido", console.Output);
    }
}
