using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class AlignmentQueryServiceTests
{
    [Fact]
    public void Query_ReturnsStatesInInputOrderAcrossElementBoundaries()
    {
        var alignment = BuildAlignment();
        var query = new AlignmentQueryInput(alignment, new List<double> { 160, 0, 100, 220, 9999 });

        var result = new AlignmentQueryService().Query(query);

        Assert.Equal(new[] { 160d, 0d, 100d, 220d, 9999d }, result.Points.Select(point => point.Chainage));
        Assert.True(result.Points[0].IsInsideAlignment);
        Assert.Equal("TANGENT", result.Points[2].ElementType);
        Assert.False(result.Points[4].IsInsideAlignment);
        Assert.Contains(result.Warnings, warning => warning.Contains("outside", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Query_ReturnsMatchingCoordinatesAtExactBoundary()
    {
        var alignment = BuildAlignment();
        var result = new AlignmentQueryService().Query(new AlignmentQueryInput(alignment, new List<double> { 100, 160 }));

        Assert.Equal(result.Points[0].X, alignment.Elements[0].EndState.X, 8);
        Assert.Equal(result.Points[1].X, alignment.Elements[1].EndState.X, 8);
        Assert.True(double.IsPositiveInfinity(result.Points[0].Radius));
    }

    [Fact]
    public void Query_ReturnsAlignmentEndState()
    {
        var alignment = BuildAlignment();
        var endChainage = alignment.Elements[^1].EndState.Chainage;

        var result = new AlignmentQueryService().Query(new AlignmentQueryInput(alignment, new List<double> { endChainage }));

        Assert.True(result.Points[0].IsInsideAlignment);
        Assert.Equal(alignment.Elements[^1].EndState.X, result.Points[0].X, 8);
        Assert.Equal(alignment.Elements[^1].EndState.Y, result.Points[0].Y, 8);
    }

    private static HorizontalAlignment BuildAlignment()
    {
        var input = new HorizontalAlignmentInput(
            "Route-A",
            0,
            1000,
            1000,
            0,
            new List<AlignmentElementDefinition>
            {
                new("TANGENT", "T1", 100, null, null, null, false),
                new("CLOTHOID", "S1", 60, 300, null, "LEFT", false),
                new("ARC", "C1", null, 300, 35, "LEFT", false)
            });

        return new HorizontalAlignmentBuilder().Build(input).Alignment!;
    }
}
