using Workshop.Tools.Clarion;

namespace Workshop.Tests.Tools.Clarion;

public class ClarionConverterTests
{
    [Theory]
    [InlineData("1", "29/12/1800")]
    [InlineData("4", "01/01/1801")]
    [InlineData("81453", "01/01/2024")]
    [InlineData("82455", "29/09/2026")]
    [InlineData("2994626", "31/12/9999")]
    public void ConvertDate_WorksInBothDirections(string clarion, string date)
    {
        Assert.Equal(date, ClarionConverter.ConvertDate(clarion));
        Assert.Equal(clarion, ClarionConverter.ConvertDate(date));
    }

    [Fact]
    public void ConvertDate_AcceptsDayAndMonthWithoutLeadingZero()
    {
        Assert.Equal("81453", ClarionConverter.ConvertDate("1/1/2024"));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2994627")]
    [InlineData("99999999999")]
    [InlineData("28/12/1800")]
    [InlineData("31/02/2024")]
    [InlineData("2024-01-01")]
    [InlineData("abc")]
    [InlineData("")]
    public void ConvertDate_InvalidValue_ReturnsNull(string text)
    {
        Assert.Null(ClarionConverter.ConvertDate(text));
    }
}
