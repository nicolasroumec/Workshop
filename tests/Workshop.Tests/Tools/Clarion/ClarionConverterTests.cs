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

    [Theory]
    [InlineData("1", "00:00:00.00")]
    [InlineData("100", "00:00:00.99")]
    [InlineData("5964001", "16:34:00.00")]
    [InlineData("4599263", "12:46:32.62")]
    [InlineData("8640000", "23:59:59.99")]
    public void ConvertTime_WorksInBothDirections(string clarion, string time)
    {
        Assert.Equal(time, ClarionConverter.ConvertTime(clarion));
        Assert.Equal(clarion, ClarionConverter.ConvertTime(time));
    }

    [Theory]
    [InlineData("16:34", "5964001")]
    [InlineData("16:34:00", "5964001")]
    [InlineData("7:05", "2550001")]
    [InlineData("12:46:32.6", "4599261")]
    public void ConvertTime_AcceptsShorterFormats(string time, string clarion)
    {
        Assert.Equal(clarion, ClarionConverter.ConvertTime(time));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("8640001")]
    [InlineData("24:00")]
    [InlineData("12:60")]
    [InlineData("12:00:00.123")]
    [InlineData("abc")]
    [InlineData("")]
    public void ConvertTime_InvalidValue_ReturnsNull(string text)
    {
        Assert.Null(ClarionConverter.ConvertTime(text));
    }

    [Fact]
    public void ConvertDateTime_WorksInBothDirections()
    {
        Assert.Equal("29/09/2026 12:46:32.62", ClarionConverter.ConvertDateTime("82455 4599263"));
        Assert.Equal("82455 4599263", ClarionConverter.ConvertDateTime("29/09/2026 12:46:32.62"));
    }

    [Theory]
    [InlineData("82455")]
    [InlineData("82455 0")]
    [InlineData("0 1")]
    [InlineData("1 2 3")]
    public void ConvertDateTime_InvalidValue_ReturnsNull(string text)
    {
        Assert.Null(ClarionConverter.ConvertDateTime(text));
    }
}
