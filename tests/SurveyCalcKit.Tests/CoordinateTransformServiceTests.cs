using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class CoordinateTransformServiceTests
{
    [Fact]
    public void Transform_AppliesTranslation()
    {
        var transformer = new CoordinateTransformService();
        var points = new[] { new PointRecord("P1", 1, 2, 3) };

        var point = Assert.Single(transformer.Transform(points, dx: 10, dy: -5));

        Assert.Equal("P1", point.Name);
        Assert.Equal(11, point.X, 6);
        Assert.Equal(-3, point.Y, 6);
        Assert.Equal(3, point.H);
    }

    [Fact]
    public void Transform_AppliesScale()
    {
        var transformer = new CoordinateTransformService();
        var points = new[] { new PointRecord("P1", 2, -3) };

        var point = Assert.Single(transformer.Transform(points, dx: 0, dy: 0, scale: 2));

        Assert.Equal(4, point.X, 6);
        Assert.Equal(-6, point.Y, 6);
    }

    [Fact]
    public void Transform_AppliesRotationInDegrees()
    {
        var transformer = new CoordinateTransformService();
        var points = new[] { new PointRecord("P1", 1, 0) };

        var point = Assert.Single(transformer.Transform(points, dx: 0, dy: 0, scale: 1, rotationAngleDegrees: 90));

        Assert.Equal(0, point.X, 6);
        Assert.Equal(1, point.Y, 6);
    }
}
