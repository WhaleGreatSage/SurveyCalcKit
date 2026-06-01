using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class ParseServiceTests
{
    [Fact]
    public void ParsePoints_ParsesSpaceSeparatedRows()
    {
        var parser = new ParseService();

        var result = parser.ParsePoints("P1 100.000 200.000");

        Assert.True(result.IsSuccess);
        var point = Assert.Single(result.Points);
        Assert.Equal("P1", point.Name);
        Assert.Equal(100.000, point.X, 3);
        Assert.Equal(200.000, point.Y, 3);
        Assert.Null(point.H);
    }

    [Fact]
    public void ParsePoints_ParsesCommaSeparatedRows()
    {
        var parser = new ParseService();

        var result = parser.ParsePoints("P1,100.000,200.000");

        Assert.True(result.IsSuccess);
        var point = Assert.Single(result.Points);
        Assert.Equal("P1", point.Name);
        Assert.Equal(100.000, point.X, 3);
        Assert.Equal(200.000, point.Y, 3);
    }

    [Fact]
    public void ParsePoints_ParsesOptionalElevation()
    {
        var parser = new ParseService();

        var result = parser.ParsePoints("P1 100.000 200.000 15.230");

        Assert.True(result.IsSuccess);
        var point = Assert.Single(result.Points);
        Assert.Equal(15.230, point.H!.Value, 3);
    }

    [Fact]
    public void ParsePoints_InvalidNumericFieldReturnsClearError()
    {
        var parser = new ParseService();

        var result = parser.ParsePoints("P1 abc 200.000");

        Assert.False(result.IsSuccess);
        var error = Assert.Single(result.Errors);
        Assert.Equal(1, error.LineNumber);
        Assert.Contains("X", error.Message);
        Assert.Contains("abc", error.Message);
    }
}
