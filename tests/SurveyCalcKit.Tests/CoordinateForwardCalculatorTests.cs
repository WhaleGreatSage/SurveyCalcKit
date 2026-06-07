using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class CoordinateForwardCalculatorTests
{
    [Fact]
    public void Calculate_ComputesEndpointFromStartAzimuthAndDistance()
    {
        var calculator = new CoordinateForwardCalculator();
        var input = new CoordinateForwardInput("P1", 1000, 1000, 53.130102, 50, "P2");

        var result = calculator.Calculate(input);

        Assert.Equal(53.130102, result.AzimuthDegrees, 6);
        Assert.Equal(30, result.DeltaX, 5);
        Assert.Equal(40, result.DeltaY, 5);
        Assert.Equal("P2", result.EndPointName);
        Assert.Equal(1030, result.EndX, 5);
        Assert.Equal(1040, result.EndY, 5);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Calculate_NormalizesAzimuthToZeroTo360Degrees()
    {
        var calculator = new CoordinateForwardCalculator();
        var input = new CoordinateForwardInput("P1", 0, 0, -90, 10, "P2");

        var result = calculator.Calculate(input);

        Assert.Equal(270, result.AzimuthDegrees, 6);
        Assert.Equal(0, result.DeltaX, 6);
        Assert.Equal(-10, result.DeltaY, 6);
    }

    [Fact]
    public void Calculate_SupportsZeroDistance()
    {
        var calculator = new CoordinateForwardCalculator();
        var input = new CoordinateForwardInput("P1", 10, 20, 45, 0, "P2");

        var result = calculator.Calculate(input);

        Assert.Equal(0, result.Distance, 6);
        Assert.Equal(10, result.EndX, 6);
        Assert.Equal(20, result.EndY, 6);
    }

    [Fact]
    public void Calculate_AddsWarningForNegativeDistance()
    {
        var calculator = new CoordinateForwardCalculator();
        var input = new CoordinateForwardInput("P1", 10, 20, 45, -5, "P2");

        var result = calculator.Calculate(input);

        Assert.Contains(result.Warnings, warning => warning.Contains("negative", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseCoordinateForward_ParsesSpaceSeparatedFormat()
    {
        var parser = new ParseService();

        var result = parser.ParseCoordinateForward(
            """
            START P1 1000.000 1000.000
            AZIMUTH 53.130102
            DISTANCE 50.000
            END P2
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("P1", result.Input!.StartPointName);
        Assert.Equal(1000, result.Input.StartX, 3);
        Assert.Equal(1000, result.Input.StartY, 3);
        Assert.Equal(53.130102, result.Input.AzimuthDegrees, 6);
        Assert.Equal(50, result.Input.Distance, 3);
        Assert.Equal("P2", result.Input.EndPointName);
    }

    [Fact]
    public void ParseCoordinateForward_ParsesCommaSeparatedFormat()
    {
        var parser = new ParseService();

        var result = parser.ParseCoordinateForward(
            """
            START,P1,1000.000,1000.000
            AZIMUTH,53.130102
            DISTANCE,50.000
            END,P2
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("P1", result.Input!.StartPointName);
        Assert.Equal("P2", result.Input.EndPointName);
        Assert.Equal(50, result.Input.Distance, 3);
    }
}
