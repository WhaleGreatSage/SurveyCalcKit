using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class CenterlineOffsetCalculatorTests
{
    [Fact]
    public void Calculate_SelectsNearestSegmentAndSignsOffsetsBySide()
    {
        var result = new CenterlineOffsetCalculator().Calculate(CreateInput());

        Assert.Equal(3, result.Results.Count);
        Assert.Equal(50, result.Results[0].Chainage, 6);
        Assert.Equal("Left", result.Results[0].Side);
        Assert.Equal(20, result.Results[0].SignedOffset, 6);
        Assert.Equal(1, result.Results[1].SegmentIndex);
        Assert.Equal("Right", result.Results[1].Side);
    }

    [Fact]
    public void Calculate_HandlesVertexAndReportsNonIncreasingChainage()
    {
        var input = new CenterlineOffsetInput(
            new List<CenterlinePoint>
            {
                new("A", 0, 0, 0),
                new("B", 100, 100, 0),
                new("C", 100, 150, 50)
            },
            new List<PointRecord> { new("P", 100, 0) });

        var result = new CenterlineOffsetCalculator().Calculate(input);

        Assert.Equal(100, result.Results[0].Chainage, 6);
        Assert.Contains(result.Warnings, warning => warning.Contains("non-increasing", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_WarnsForZeroLengthSegment()
    {
        var input = new CenterlineOffsetInput(
            new List<CenterlinePoint>
            {
                new("A", 0, 0, 0),
                new("B", 10, 0, 0),
                new("C", 20, 20, 0)
            },
            new List<PointRecord> { new("P", 10, 5) });

        var result = new CenterlineOffsetCalculator().Calculate(input);

        Assert.Single(result.Results);
        Assert.Contains(result.Warnings, warning => warning.Contains("zero length", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseCenterlineOffset_ParsesCenterlineAndTargets()
    {
        var parseResult = new ParseService().ParseCenterlineOffset(
            """
            CENTERLINE
            A 0.000 1000.000 1000.000
            B 100.000 1100.000 1000.000
            TARGETS
            P1 1050.000 1020.000
            """);

        Assert.True(parseResult.IsSuccess);
        Assert.Equal(2, parseResult.Input!.CenterlinePoints.Count);
        Assert.Single(parseResult.Input.TargetPoints);
    }

    private static CenterlineOffsetInput CreateInput() =>
        new(
            new List<CenterlinePoint>
            {
                new("A", 0, 1000, 1000),
                new("B", 100, 1100, 1000),
                new("C", 180, 1160, 1050),
                new("D", 260, 1200, 1120)
            },
            new List<PointRecord>
            {
                new("P1", 1050, 1020),
                new("P2", 1130, 1020),
                new("P3", 1210, 1100)
            });
}
