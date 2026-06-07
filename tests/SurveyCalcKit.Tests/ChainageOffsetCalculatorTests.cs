using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class ChainageOffsetCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsLeftSideForTargetLeftOfBaseline()
    {
        var result = Calculate(50, 25);

        Assert.Equal(100, result.BaselineLength, 6);
        Assert.Equal(0.5, result.ProjectionRatio, 6);
        Assert.Equal(50, result.Chainage, 6);
        Assert.Equal(25, result.Offset, 6);
        Assert.Equal("Left", result.Side);
        Assert.True(result.ProjectionInsideSegment);
        Assert.Equal(50, result.ProjectionX, 6);
        Assert.Equal(0, result.ProjectionY, 6);
    }

    [Fact]
    public void Calculate_ReturnsRightSideForTargetRightOfBaseline()
    {
        var result = Calculate(50, -25);

        Assert.Equal("Right", result.Side);
        Assert.Equal(25, result.Offset, 6);
    }

    [Fact]
    public void Calculate_ReturnsOnLineForTargetOnBaseline()
    {
        var result = Calculate(50, 0);

        Assert.Equal("OnLine", result.Side);
        Assert.Equal(0, result.Offset, 6);
        Assert.Contains(result.Warnings, warning => warning.Contains("line", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_FlagsProjectionOutsideSegment()
    {
        var result = Calculate(150, 10);

        Assert.False(result.ProjectionInsideSegment);
        Assert.Equal(150, result.Chainage, 6);
        Assert.Contains(result.Warnings, warning => warning.Contains("outside", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_AddsWarningForZeroBaselineLength()
    {
        var calculator = new ChainageOffsetCalculator();
        var input = new ChainageOffsetInput("A", 10, 10, "B", 10, 10, "P1", 12, 13, 100);

        var result = calculator.Calculate(input);

        Assert.Equal(0, result.BaselineLength, 6);
        Assert.Equal(100, result.Chainage, 6);
        Assert.Contains(result.Warnings, warning => warning.Contains("zero", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_UsesNonZeroStartChainage()
    {
        var calculator = new ChainageOffsetCalculator();
        var input = new ChainageOffsetInput("A", 0, 0, "B", 100, 0, "P1", 50, 25, 1000);

        var result = calculator.Calculate(input);

        Assert.Equal(1050, result.Chainage, 6);
    }

    [Fact]
    public void ParseChainageOffset_ParsesSpaceSeparatedFormat()
    {
        var parser = new ParseService();

        var result = parser.ParseChainageOffset(
            """
            BASELINE A 1000.000 1000.000 B 1100.000 1000.000
            START_CHAINAGE 0.000
            POINT P1 1050.000 1025.000
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("A", result.Input!.BaselineStartName);
        Assert.Equal("B", result.Input.BaselineEndName);
        Assert.Equal("P1", result.Input.TargetPointName);
        Assert.Equal(0, result.Input.StartChainage, 3);
    }

    [Fact]
    public void ParseChainageOffset_ParsesCommaSeparatedFormat()
    {
        var parser = new ParseService();

        var result = parser.ParseChainageOffset(
            """
            BASELINE,A,1000.000,1000.000,B,1100.000,1000.000
            START_CHAINAGE,10.000
            POINT,P1,1050.000,1025.000
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("A", result.Input!.BaselineStartName);
        Assert.Equal("B", result.Input.BaselineEndName);
        Assert.Equal("P1", result.Input.TargetPointName);
        Assert.Equal(10, result.Input.StartChainage, 3);
    }

    private static ChainageOffsetResult Calculate(double targetX, double targetY)
    {
        var calculator = new ChainageOffsetCalculator();
        var input = new ChainageOffsetInput("A", 0, 0, "B", 100, 0, "P1", targetX, targetY);

        return calculator.Calculate(input);
    }
}
