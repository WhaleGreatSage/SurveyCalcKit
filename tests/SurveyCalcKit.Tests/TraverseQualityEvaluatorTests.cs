using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class TraverseQualityEvaluatorTests
{
    [Fact]
    public void Evaluate_CalculatesRelativeClosureDenominator()
    {
        var result = EvaluateSample();

        Assert.Equal(result.TotalLength / result.LinearClosureError, result.RelativeClosureDenominator, 6);
        Assert.True(result.RelativeClosureDenominator > 2000);
    }

    [Fact]
    public void Evaluate_ComparesAgainstAllowableRelativeClosure()
    {
        var result = EvaluateSample();

        Assert.True(result.PassesLinearClosureLimit);
    }

    [Fact]
    public void Evaluate_ReportsPerfectClosure()
    {
        var evaluator = new TraverseQualityEvaluator();
        var input = new TraverseQualityInput(
            new List<PointRecord>
            {
                new("P1", 0, 0),
                new("P2", 10, 0),
                new("P3", 10, 10),
                new("P1", 0, 0)
            },
            null,
            2000,
            null,
            "Closed");

        var result = evaluator.Evaluate(input);

        Assert.Equal(0, result.LinearClosureError, 12);
        Assert.True(double.IsPositiveInfinity(result.RelativeClosureDenominator));
        Assert.Contains(result.Warnings, warning => warning.Contains("perfect", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Evaluate_CalculatesAngularClosureError()
    {
        var result = EvaluateSample();

        Assert.Equal(-1.8, result.AngularClosureErrorSeconds!.Value, 6);
        Assert.Equal(80, result.AllowableAngularClosureSeconds!.Value, 6);
    }

    [Fact]
    public void Evaluate_ReturnsAngularPassWhenWithinLimit()
    {
        var result = EvaluateSample();

        Assert.True(result.PassesAngularClosureLimit);
    }

    [Fact]
    public void Evaluate_ReturnsAngularFailWhenOutsideLimit()
    {
        var evaluator = new TraverseQualityEvaluator();
        var input = CreateSampleInput(
            observedAnglesDegrees: new List<double> { 90, 90, 90, 91 },
            allowableAngularClosureSecondsPerStation: 40);

        var result = evaluator.Evaluate(input);

        Assert.False(result.PassesAngularClosureLimit);
        Assert.Contains(result.Warnings, warning => warning.Contains("angular closure failed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Evaluate_ReturnsFailedGradeWhenLinearLimitFails()
    {
        var evaluator = new TraverseQualityEvaluator();
        var input = CreateSampleInput(allowableRelativeClosureDenominator: 10000);

        var result = evaluator.Evaluate(input);

        Assert.False(result.PassesLinearClosureLimit);
        Assert.Equal("Failed", result.QualityGrade);
    }

    [Fact]
    public void ParseTraverseQuality_ParsesValidFile()
    {
        var parser = new ParseService();

        var result = parser.ParseTraverseQuality(
            """
            POINTS
            P1 1000.000 1000.000
            P2 1100.050 1002.200
            P3 1098.600 1098.900
            P4 998.900 1097.700
            P1 1000.120 999.930
            ANGLES
            90.0020
            89.9985
            90.0040
            89.9950
            LIMITS
            RELATIVE 2000
            ANGULAR_SECONDS_PER_STATION 40
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Input!.Points.Count);
        Assert.Equal(4, result.Input.ObservedAnglesDegrees!.Count);
        Assert.Equal(2000, result.Input.AllowableRelativeClosureDenominator);
        Assert.Equal(40, result.Input.AllowableAngularClosureSecondsPerStation);
    }

    [Fact]
    public void Evaluate_AddsWarningForTooFewPoints()
    {
        var evaluator = new TraverseQualityEvaluator();
        var input = new TraverseQualityInput(
            new List<PointRecord>
            {
                new("P1", 0, 0),
                new("P2", 10, 0),
                new("P1", 0, 0)
            },
            null,
            2000,
            null,
            null);

        var result = evaluator.Evaluate(input);

        Assert.Contains(result.Warnings, warning => warning.Contains("at least 4 points", StringComparison.OrdinalIgnoreCase));
    }

    private static TraverseQualityResult EvaluateSample()
    {
        var evaluator = new TraverseQualityEvaluator();
        return evaluator.Evaluate(CreateSampleInput());
    }

    private static TraverseQualityInput CreateSampleInput(
        IReadOnlyList<double>? observedAnglesDegrees = null,
        double allowableRelativeClosureDenominator = 2000,
        double? allowableAngularClosureSecondsPerStation = 40)
    {
        return new TraverseQualityInput(
            new List<PointRecord>
            {
                new("P1", 1000.000, 1000.000),
                new("P2", 1100.050, 1002.200),
                new("P3", 1098.600, 1098.900),
                new("P4", 998.900, 1097.700),
                new("P1", 1000.120, 999.930)
            },
            observedAnglesDegrees?.ToList() ?? new List<double> { 90.0020, 89.9985, 90.0040, 89.9950 },
            allowableRelativeClosureDenominator,
            allowableAngularClosureSecondsPerStation,
            "Closed");
    }
}
