using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class ElevationCalculatorTests
{
    [Fact]
    public void CalculateDeltaH_ReturnsElevationDifference()
    {
        var calculator = new ElevationCalculator();

        var deltaH = calculator.CalculateDeltaH(
            new PointRecord("P1", 0, 0, 15.23),
            new PointRecord("P2", 0, 0, 16.00));

        Assert.Equal(0.77, deltaH!.Value, 6);
    }

    [Fact]
    public void CalculateSlopePercent_ReturnsSlopeOverHorizontalDistance()
    {
        var calculator = new ElevationCalculator();

        var slope = calculator.CalculateSlopePercent(deltaH: 1, horizontalDistance: 4);

        Assert.Equal(25, slope!.Value, 6);
    }

    [Fact]
    public void CalculateSlopePercent_ReturnsNullForZeroDistance()
    {
        var calculator = new ElevationCalculator();

        var slope = calculator.CalculateSlopePercent(deltaH: 1, horizontalDistance: 0);

        Assert.Null(slope);
    }

    [Fact]
    public void CalculateElevationClosureError_ComparesObservedAndKnownEndElevation()
    {
        var calculator = new ElevationCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0, 100.00),
            new PointRecord("P2", 1, 0, 100.40),
            new PointRecord("P3", 2, 0, 100.95)
        };

        var closureError = calculator.CalculateElevationClosureError(
            points,
            knownStartElevation: 100.00,
            knownEndElevation: 101.00);

        Assert.Equal(-0.05, closureError, 6);
    }
}
