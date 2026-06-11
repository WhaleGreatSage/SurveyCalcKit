using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class CoordinateInverseCalculatorTests
{
    [Fact]
    public void Calculate_ComputesBasicInverseValues()
    {
        var calculator = new CoordinateInverseCalculator();
        var input = new CoordinateInverseInput("P1", 1000, 1000, null, "P2", 1050, 1040, null);

        var result = calculator.Calculate(input);

        Assert.Equal("P1", result.FromPointName);
        Assert.Equal("P2", result.ToPointName);
        Assert.Equal(50, result.DeltaX, 6);
        Assert.Equal(40, result.DeltaY, 6);
        Assert.Equal(Math.Sqrt(4100), result.Distance2D, 6);
        Assert.Equal(38.659808, result.AzimuthDegrees, 6);
        Assert.Null(result.DeltaH);
        Assert.Null(result.Distance3D);
    }

    [Fact]
    public void Calculate_AddsWarningForIdenticalPoints()
    {
        var calculator = new CoordinateInverseCalculator();
        var input = new CoordinateInverseInput("P1", 1000, 1000, null, "P1", 1000, 1000, null);

        var result = calculator.Calculate(input);

        Assert.Equal(0, result.Distance2D, 6);
        Assert.Contains(result.Warnings, warning => warning.Contains("identical", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_ComputesOptionalHeightValues()
    {
        var calculator = new CoordinateInverseCalculator();
        var input = new CoordinateInverseInput("P1", 0, 0, 12.5, "P2", 3, 4, 18.5);

        var result = calculator.Calculate(input);

        Assert.Equal(6, result.DeltaH!.Value, 6);
        Assert.Equal(Math.Sqrt(61), result.Distance3D!.Value, 6);
    }

    [Fact]
    public void ParseCoordinateInverse_ParsesSpaceSeparatedFormat()
    {
        var parser = new ParseService();

        var result = parser.ParseCoordinateInverse(
            """
            FROM P1 1000.000 1000.000 12.500
            TO P2 1050.000 1040.000 13.200
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("P1", result.Input!.FromPointName);
        Assert.Equal(1000, result.Input.FromX, 3);
        Assert.Equal(12.5, result.Input.FromH!.Value, 3);
        Assert.Equal("P2", result.Input.ToPointName);
        Assert.Equal(1050, result.Input.ToX, 3);
        Assert.Equal(13.2, result.Input.ToH!.Value, 3);
    }

    [Fact]
    public void ParseCoordinateInverse_ParsesCommaSeparatedFormat()
    {
        var parser = new ParseService();

        var result = parser.ParseCoordinateInverse(
            """
            FROM,P1,1000.000,1000.000,12.500
            TO,P2,1050.000,1040.000,13.200
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("P1", result.Input!.FromPointName);
        Assert.Equal("P2", result.Input.ToPointName);
        Assert.Equal(13.2, result.Input.ToH!.Value, 3);
    }
}
