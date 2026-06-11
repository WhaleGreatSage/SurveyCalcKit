using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class BatchSegmentTableCalculatorTests
{
    [Fact]
    public void Calculate_ComputesSegmentCountAndTotalLength()
    {
        var result = CalculateSample();

        Assert.Equal(4, result.PointCount);
        Assert.Equal(3, result.SegmentCount);
        Assert.Equal(5 + 5 + 10, result.TotalLength, 6);
    }

    [Fact]
    public void Calculate_ComputesCumulativeDistance()
    {
        var result = CalculateSample();

        Assert.Equal(5, result.Rows[0].CumulativeDistance, 6);
        Assert.Equal(10, result.Rows[1].CumulativeDistance, 6);
        Assert.Equal(20, result.Rows[2].CumulativeDistance, 6);
    }

    [Fact]
    public void Calculate_ComputesSlopeWhenHeightsExist()
    {
        var result = CalculateSample();

        Assert.Equal(2, result.Rows[0].DeltaH!.Value, 6);
        Assert.Equal(40, result.Rows[0].SlopePercent!.Value, 6);
    }

    [Fact]
    public void Calculate_AddsWarningForRepeatedConsecutivePoint()
    {
        var calculator = new BatchSegmentTableCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0),
            new PointRecord("P1", 0, 0)
        };

        var result = calculator.Calculate(points);

        Assert.Single(result.Rows);
        Assert.Contains(result.Warnings, warning => warning.Contains("repeated", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_AddsWarningWhenTooFewPoints()
    {
        var calculator = new BatchSegmentTableCalculator();

        var result = calculator.Calculate(new[] { new PointRecord("P1", 0, 0) });

        Assert.Equal(1, result.PointCount);
        Assert.Equal(0, result.SegmentCount);
        Assert.Empty(result.Rows);
        Assert.Contains(result.Warnings, warning => warning.Contains("at least two", StringComparison.OrdinalIgnoreCase));
    }

    private static BatchSegmentTableResult CalculateSample()
    {
        var calculator = new BatchSegmentTableCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0, 10),
            new PointRecord("P2", 3, 4, 12),
            new PointRecord("P3", 6, 8, 13),
            new PointRecord("P4", 16, 8, 13)
        };

        return calculator.Calculate(points);
    }
}
