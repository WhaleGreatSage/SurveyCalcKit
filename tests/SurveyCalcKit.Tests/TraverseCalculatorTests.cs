using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class TraverseCalculatorTests
{
    [Fact]
    public void CalculateSegments_Calculates2DDistance()
    {
        var calculator = new TraverseCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0),
            new PointRecord("P2", 3, 4)
        };

        var segment = Assert.Single(calculator.CalculateSegments(points));

        Assert.Equal(3, segment.Dx, 6);
        Assert.Equal(4, segment.Dy, 6);
        Assert.Equal(5, segment.Distance2D, 6);
    }

    [Fact]
    public void CalculateSegments_Calculates3DDistance()
    {
        var calculator = new TraverseCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0, 10),
            new PointRecord("P2", 3, 4, 22)
        };

        var segment = Assert.Single(calculator.CalculateSegments(points));

        Assert.Equal(12, segment.DeltaH!.Value, 6);
        Assert.Equal(13, segment.Distance3D!.Value, 6);
    }

    [Theory]
    [InlineData(1, 1, 45)]
    [InlineData(-1, 1, 135)]
    [InlineData(-1, -1, 225)]
    [InlineData(1, -1, 315)]
    public void CalculateSegments_CalculatesAzimuthInMultipleQuadrants(double x, double y, double expectedAzimuth)
    {
        var calculator = new TraverseCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0),
            new PointRecord("P2", x, y)
        };

        var segment = Assert.Single(calculator.CalculateSegments(points));

        Assert.Equal(expectedAzimuth, segment.AzimuthDegrees, 6);
    }

    [Fact]
    public void CalculateTotal2DLength_SumsSegmentDistances()
    {
        var calculator = new TraverseCalculator();
        var points = new[]
        {
            new PointRecord("P1", 0, 0),
            new PointRecord("P2", 3, 4),
            new PointRecord("P3", 6, 8)
        };

        var segments = calculator.CalculateSegments(points);
        var total = calculator.CalculateTotal2DLength(segments);

        Assert.Equal(10, total, 6);
    }
}
