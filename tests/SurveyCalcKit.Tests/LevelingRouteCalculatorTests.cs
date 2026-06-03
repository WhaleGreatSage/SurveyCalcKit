using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class LevelingRouteCalculatorTests
{
    [Fact]
    public void ParseLevelingRoute_ParsesValidRoute()
    {
        var parser = new ParseService();

        var result = parser.ParseLevelingRoute(CreateSampleText());

        Assert.True(result.IsSuccess);
        Assert.Equal("BM1", result.Route!.StartBenchmarkName);
        Assert.Equal(100.000, result.Route.StartElevation, 3);
        Assert.Equal("BM2", result.Route.EndBenchmarkName);
        Assert.Equal(100.480, result.Route.EndElevation, 3);
        Assert.Equal(3, result.Route.Observations.Count);
        Assert.Equal("P1", result.Route.Observations[0].PointName);
        Assert.Equal(1.235, result.Route.Observations[0].Backsight, 3);
        Assert.Equal(0.865, result.Route.Observations[0].Foresight, 3);
    }

    [Fact]
    public void Calculate_ComputesSumBacksight()
    {
        var result = CalculateSample();

        Assert.Equal(3.335, result.SumBacksight, 6);
    }

    [Fact]
    public void Calculate_ComputesSumForesight()
    {
        var result = CalculateSample();

        Assert.Equal(2.855, result.SumForesight, 6);
    }

    [Fact]
    public void Calculate_ComputesObservedHeightDifference()
    {
        var result = CalculateSample();

        Assert.Equal(0.480, result.ObservedHeightDifference, 6);
    }

    [Fact]
    public void Calculate_ComputesKnownHeightDifference()
    {
        var result = CalculateSample();

        Assert.Equal(0.480, result.KnownHeightDifference, 6);
    }

    [Fact]
    public void Calculate_ComputesClosureError()
    {
        var input = CreateSampleInput() with { EndElevation = 100.500 };
        var calculator = new LevelingRouteCalculator();

        var result = calculator.Calculate(input);

        Assert.Equal(-0.020, result.ClosureError, 6);
    }

    [Fact]
    public void Calculate_ComputesCorrectionPerStation()
    {
        var input = CreateSampleInput() with { EndElevation = 100.500 };
        var calculator = new LevelingRouteCalculator();

        var result = calculator.Calculate(input);

        Assert.Equal(0.020 / 3.0, result.CorrectionPerStation, 6);
    }

    [Fact]
    public void Calculate_ComputesAdjustedElevations()
    {
        var input = CreateSampleInput() with { EndElevation = 100.500 };
        var calculator = new LevelingRouteCalculator();

        var result = calculator.Calculate(input);

        Assert.Equal(3, result.Points.Count);
        Assert.Equal("P1", result.Points[0].PointName);
        Assert.Equal(100.370, result.Points[0].RawElevation, 6);
        Assert.Equal(0.020 / 3.0, result.Points[0].Correction, 6);
        Assert.Equal(100.370 + 0.020 / 3.0, result.Points[0].AdjustedElevation, 6);
        Assert.Equal(100.480, result.Points[^1].RawElevation, 6);
        Assert.Equal(0.020, result.Points[^1].Correction, 6);
        Assert.Equal(100.500, result.Points[^1].AdjustedElevation, 6);
    }

    [Fact]
    public void ParseLevelingRoute_ReturnsErrorForEmptyInput()
    {
        var parser = new ParseService();

        var result = parser.ParseLevelingRoute("");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Message.Contains("START", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseLevelingRoute_ReturnsErrorForInvalidNumericField()
    {
        var parser = new ParseService();

        var result = parser.ParseLevelingRoute(
            """
            START BM1 100.000
            P1 abc 0.865
            END BM2 100.480
            """);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Message.Contains("Backsight", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.Message.Contains("abc", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_AddsWarningForNegativeSightValue()
    {
        var input = new LevelingRouteInput(
            "BM1",
            100,
            "BM2",
            101,
            new List<LevelingObservation>
            {
                new("P1", -1.0, 0.5)
            });
        var calculator = new LevelingRouteCalculator();

        var result = calculator.Calculate(input);

        Assert.Contains(result.Warnings, warning => warning.Contains("negative", StringComparison.OrdinalIgnoreCase));
    }

    private static LevelingRouteResult CalculateSample()
    {
        var parser = new ParseService();
        var parseResult = parser.ParseLevelingRoute(CreateSampleText());
        var calculator = new LevelingRouteCalculator();

        return calculator.Calculate(parseResult.Route!);
    }

    private static LevelingRouteInput CreateSampleInput()
    {
        return new LevelingRouteInput(
            "BM1",
            100.000,
            "BM2",
            100.480,
            new List<LevelingObservation>
            {
                new("P1", 1.235, 0.865),
                new("P2", 1.120, 0.940),
                new("P3", 0.980, 1.050)
            });
    }

    private static string CreateSampleText()
    {
        return
            """
            START BM1 100.000
            P1 1.235 0.865
            P2 1.120 0.940
            P3 0.980 1.050
            END BM2 100.480
            """;
    }
}
