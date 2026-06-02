using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class ClosedTraverseCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsClosureValuesForValidClosedTraverse()
    {
        var calculator = new ClosedTraverseCalculator();
        var points = CreateSampleTraverse();

        var result = calculator.Calculate(points);

        Assert.Equal("P1", result.StartPointName);
        Assert.Equal("P1", result.EndPointName);
        Assert.Equal(0.080, result.Fx, 6);
        Assert.Equal(-0.060, result.Fy, 6);
        Assert.Equal(0.100, result.ClosureError, 6);
        Assert.True(result.TotalLength > 180);
        Assert.True(result.RelativeClosureRatio > 1800);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Calculate_ReportsNonZeroClosureError()
    {
        var calculator = new ClosedTraverseCalculator();

        var result = calculator.Calculate(CreateSampleTraverse());

        Assert.NotEqual(0, result.ClosureError);
        Assert.Equal(Math.Sqrt(0.080 * 0.080 + -0.060 * -0.060), result.ClosureError, 6);
    }

    [Fact]
    public void Calculate_ReportsPerfectClosure()
    {
        var calculator = new ClosedTraverseCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0),
            new PointRecord("P2", 10, 0),
            new PointRecord("P3", 10, 10),
            new PointRecord("P1", 0, 0)
        };

        var result = calculator.Calculate(points);

        Assert.Equal(0, result.ClosureError, 12);
        Assert.True(double.IsPositiveInfinity(result.RelativeClosureRatio));
        Assert.Contains(result.Warnings, warning => warning.Contains("perfect", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_ComputesRelativeClosureRatio()
    {
        var calculator = new ClosedTraverseCalculator();

        var result = calculator.Calculate(CreateSampleTraverse());

        Assert.Equal(result.TotalLength / result.ClosureError, result.RelativeClosureRatio, 6);
    }

    [Fact]
    public void Calculate_DistributesBowditchCorrectionsBySegmentLength()
    {
        var calculator = new ClosedTraverseCalculator();

        var result = calculator.Calculate(CreateSampleTraverse());
        var firstSegment = result.AdjustedSegments[0];
        var expectedCorrectionX = -result.Fx * firstSegment.Distance2D / result.TotalLength;
        var expectedCorrectionY = -result.Fy * firstSegment.Distance2D / result.TotalLength;

        Assert.Equal(expectedCorrectionX, firstSegment.CorrectionDx, 6);
        Assert.Equal(expectedCorrectionY, firstSegment.CorrectionDy, 6);
        Assert.Equal(firstSegment.OriginalDx + expectedCorrectionX, firstSegment.AdjustedDx, 6);
        Assert.Equal(firstSegment.OriginalDy + expectedCorrectionY, firstSegment.AdjustedDy, 6);
    }

    [Fact]
    public void Calculate_AdjustedFinalCoordinateMatchesStartWithinTolerance()
    {
        var calculator = new ClosedTraverseCalculator();

        var result = calculator.Calculate(CreateSampleTraverse());
        var finalPoint = result.AdjustedPoints[^1];

        Assert.Equal(result.StartX, finalPoint.AdjustedX, 6);
        Assert.Equal(result.StartY, finalPoint.AdjustedY, 6);
    }

    [Fact]
    public void Calculate_ReturnsWarningWhenTooFewPoints()
    {
        var calculator = new ClosedTraverseCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0),
            new PointRecord("P2", 10, 0),
            new PointRecord("P1", 0, 0)
        };

        var result = calculator.Calculate(points);

        Assert.Contains(result.Warnings, warning => warning.Contains("at least 4 points", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(result.AdjustedSegments);
    }

    [Fact]
    public void Calculate_ProtectsAgainstZeroTotalLength()
    {
        var calculator = new ClosedTraverseCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0),
            new PointRecord("P2", 0, 0),
            new PointRecord("P3", 0, 0),
            new PointRecord("P1", 0, 0)
        };

        var result = calculator.Calculate(points);

        Assert.Equal(0, result.TotalLength);
        Assert.Contains(result.Warnings, warning => warning.Contains("total length is zero", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(result.AdjustedSegments);
    }

    private static IReadOnlyList<PointRecord> CreateSampleTraverse()
    {
        return new[]
        {
            new PointRecord("P1", 1000.000, 1000.000, 12.500),
            new PointRecord("P2", 1050.120, 1001.350, 12.760),
            new PointRecord("P3", 1048.900, 1042.800, 13.100),
            new PointRecord("P4", 998.750, 1041.600, 12.880),
            new PointRecord("P1", 1000.080, 999.940, 12.500)
        };
    }
}
