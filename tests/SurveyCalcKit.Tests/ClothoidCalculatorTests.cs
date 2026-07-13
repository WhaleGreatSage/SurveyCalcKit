using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class ClothoidCalculatorTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void Calculate_ReturnsStartStateAtZeroDistance()
    {
        var result = new ClothoidCalculator().Calculate(CreateInput("Left"));
        var point = result.Points.Single(point => point.DistanceFromStart == 0);

        Assert.Equal(1000, point.X, Tolerance);
        Assert.Equal(1000, point.Y, Tolerance);
        Assert.Equal(20, point.AzimuthDegrees, Tolerance);
        Assert.Equal(0, point.Curvature, Tolerance);
    }

    [Fact]
    public void Calculate_ReachesExpectedFinalCurvatureAndHeadingChange()
    {
        var result = new ClothoidCalculator().Calculate(CreateInput("Left"));
        var point = result.Points.Single(point => point.DistanceFromStart == 80);

        Assert.Equal(1.0 / 300.0, point.Curvature, Tolerance);
        Assert.Equal(20 + 80.0 / (2 * 300.0) * 180 / Math.PI, point.AzimuthDegrees, Tolerance);
    }

    [Fact]
    public void Calculate_MirrorsLeftAndRightCurvesAroundStartTangent()
    {
        var left = new ClothoidCalculator().Calculate(CreateInput("Left"));
        var right = new ClothoidCalculator().Calculate(CreateInput("Right"));

        var leftEnd = left.Points.Single(point => point.DistanceFromStart == 80);
        var rightEnd = right.Points.Single(point => point.DistanceFromStart == 80);

        var heading = 20 * Math.PI / 180.0;
        var forwardX = Math.Cos(heading);
        var forwardY = Math.Sin(heading);
        var leftX = -Math.Sin(heading);
        var leftY = Math.Cos(heading);
        var leftForward = (leftEnd.X - 1000) * forwardX + (leftEnd.Y - 1000) * forwardY;
        var rightForward = (rightEnd.X - 1000) * forwardX + (rightEnd.Y - 1000) * forwardY;
        var leftOffset = (leftEnd.X - 1000) * leftX + (leftEnd.Y - 1000) * leftY;
        var rightOffset = (rightEnd.X - 1000) * leftX + (rightEnd.Y - 1000) * leftY;

        Assert.Equal(leftForward, rightForward, 6);
        Assert.Equal(leftOffset, -rightOffset, 6);
    }

    [Fact]
    public void Calculate_UsesFiniteNumericalCoordinates()
    {
        var result = new ClothoidCalculator().Calculate(CreateInput("Right"));

        Assert.All(result.Points, point =>
        {
            Assert.True(double.IsFinite(point.X));
            Assert.True(double.IsFinite(point.Y));
            Assert.True(double.IsFinite(point.AzimuthDegrees));
        });
    }

    [Fact]
    public void Calculate_MatchesFixedNumericalReferenceAtSpiralEnd()
    {
        var result = new ClothoidCalculator().Calculate(CreateInput("Left"));
        var point = result.Points.Single(point => point.DistanceFromStart == 80);

        // Reference values are for the documented 300 m radius, 80 m spiral, and 20 degree start azimuth.
        Assert.Equal(1073.827346, point.X, 5);
        Assert.Equal(1030.649898, point.Y, 5);
    }

    [Fact]
    public void Calculate_ReturnsWarningsForInvalidRadiusAndLength()
    {
        var input = new ClothoidInput("S1", 0, 0, 0, 0, 0, "Left", new List<double> { 0 });

        var result = new ClothoidCalculator().Calculate(input);

        Assert.Contains(result.Warnings, warning => warning.Contains("Radius", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Contains("length", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseClothoid_ParsesSampleFormat()
    {
        var parseResult = new ParseService().ParseClothoid(
            """
            CLOTHOID S1
            START 1000.000 1000.000
            AZIMUTH 20.0000
            RADIUS 300.000
            LENGTH 80.000
            DIRECTION RIGHT
            DISTANCES
            0
            80
            """);

        Assert.True(parseResult.IsSuccess);
        Assert.Equal("S1", parseResult.Input!.CurveName);
        Assert.Equal(2, parseResult.Input.DistancesFromStart.Count);
    }

    private static ClothoidInput CreateInput(string direction) =>
        new("S1", 1000, 1000, 20, 300, 80, direction, new List<double> { 0, 40, 80 });
}
