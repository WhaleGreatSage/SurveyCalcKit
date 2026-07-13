using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class HorizontalAlignmentTests
{
    [Fact]
    public void Build_PropagatesContinuousTangentClothoidArcAndReverseClothoid()
    {
        var result = new HorizontalAlignmentBuilder().Build(CreateInput());

        Assert.NotNull(result.Alignment);
        Assert.Equal(5, result.Elements.Count);
        Assert.Equal(result.Elements[0].EndX, result.Elements[1].StartX, 8);
        Assert.Equal(result.Elements[1].EndY, result.Elements[2].StartY, 8);
        Assert.Equal(result.Elements[1].EndAzimuthDegrees, result.Elements[2].StartAzimuthDegrees, 8);
        Assert.Equal(result.Elements[2].EndCurvature, result.Elements[3].StartCurvature, 8);
        Assert.Equal(370 + 300 * 35 * Math.PI / 180.0, result.TotalLength, 6);
    }

    [Fact]
    public void Build_ComputesTangentAndArcEndpoints()
    {
        var input = new HorizontalAlignmentInput(
            "Simple",
            0,
            0,
            0,
            0,
            new List<AlignmentElementDefinition>
            {
                new("TANGENT", "T1", 100, null, null, null, false),
                new("ARC", "C1", null, 100, 90, "LEFT", false)
            });

        var result = new HorizontalAlignmentBuilder().Build(input);

        Assert.Equal(100, result.Elements[0].EndX, 6);
        Assert.Equal(0, result.Elements[0].EndY, 6);
        Assert.Equal(200, result.Elements[1].EndX, 6);
        Assert.Equal(100, result.Elements[1].EndY, 6);
    }

    [Fact]
    public void Build_ReturnsWarningForInvalidElementOrder()
    {
        var input = new HorizontalAlignmentInput(
            "Invalid",
            0,
            0,
            0,
            0,
            new List<AlignmentElementDefinition>
            {
                new("ARC", "C1", null, 300, 20, "LEFT", false),
                new("TANGENT", "T1", 20, null, null, null, false)
            });

        var result = new HorizontalAlignmentBuilder().Build(input);

        Assert.Contains(result.Warnings, warning => warning.Contains("curvature", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Build_ReturnsWarningForInvalidRadius()
    {
        var input = new HorizontalAlignmentInput(
            "Invalid radius",
            0,
            0,
            0,
            0,
            new List<AlignmentElementDefinition>
            {
                new("CLOTHOID", "S1", 60, 0, null, "LEFT", false)
            });

        var result = new HorizontalAlignmentBuilder().Build(input);

        Assert.Contains(result.Warnings, warning => warning.Contains("radius", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseHorizontalAlignment_ParsesCompleteSample()
    {
        var parseResult = new ParseService().ParseHorizontalAlignment(
            """
            ALIGNMENT Route-A
            START_CHAINAGE 0.000
            START 1000.000 1000.000
            AZIMUTH 15.0000

            ELEMENT TANGENT T1 LENGTH 100.000
            ELEMENT CLOTHOID S1 LENGTH 60.000 RADIUS 300.000 DIRECTION LEFT
            ELEMENT ARC C1 RADIUS 300.000 ANGLE 35.0000 DIRECTION LEFT
            ELEMENT CLOTHOID S2 LENGTH 60.000 RADIUS 300.000 DIRECTION LEFT REVERSE
            ELEMENT TANGENT T2 LENGTH 150.000
            """);

        Assert.True(parseResult.IsSuccess);
        Assert.Equal(5, parseResult.Input!.Elements.Count);
        Assert.True(parseResult.Input.Elements[3].Reverse);
    }

    private static HorizontalAlignmentInput CreateInput() =>
        new(
            "Route-A",
            0,
            1000,
            1000,
            15,
            new List<AlignmentElementDefinition>
            {
                new("TANGENT", "T1", 100, null, null, null, false),
                new("CLOTHOID", "S1", 60, 300, null, "LEFT", false),
                new("ARC", "C1", null, 300, 35, "LEFT", false),
                new("CLOTHOID", "S2", 60, 300, null, "LEFT", true),
                new("TANGENT", "T2", 150, null, null, null, false)
            });
}
