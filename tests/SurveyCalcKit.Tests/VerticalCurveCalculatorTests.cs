using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class VerticalCurveCalculatorTests
{
    [Fact]
    public void Calculate_ComputesValidVerticalCurve()
    {
        var result = new VerticalCurveCalculator().Calculate(CreateInput());

        Assert.Equal("VC1", result.CurveName);
        Assert.Equal(1250, result.PviChainage, 6);
        Assert.Equal(56.8, result.PviElevation, 6);
        Assert.Equal(-3.5, result.AlgebraicGradeDifferencePercent, 6);
        Assert.Equal("Crest", result.CurveType);
        Assert.Equal(5, result.Points.Count);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Calculate_ComputesPvcAndPvtChainages()
    {
        var result = new VerticalCurveCalculator().Calculate(CreateInput());

        Assert.Equal(1150, result.PvcChainage, 6);
        Assert.Equal(1350, result.PvtChainage, 6);
    }

    [Fact]
    public void Calculate_ComputesPvcElevation()
    {
        var result = new VerticalCurveCalculator().Calculate(CreateInput());

        Assert.Equal(54.8, result.PvcElevation, 6);
    }

    [Fact]
    public void Calculate_ComputesCurveElevationAtPvc()
    {
        var result = new VerticalCurveCalculator().Calculate(CreateInput());

        Assert.Equal(54.8, result.Points.Single(point => point.Chainage == 1150).CurveElevation, 6);
    }

    [Fact]
    public void Calculate_ComputesCurveElevationAtPvi()
    {
        var result = new VerticalCurveCalculator().Calculate(CreateInput());

        Assert.Equal(55.925, result.Points.Single(point => point.Chainage == 1250).CurveElevation, 6);
    }

    [Fact]
    public void Calculate_ComputesCurveElevationAtPvt()
    {
        var result = new VerticalCurveCalculator().Calculate(CreateInput());

        Assert.Equal(55.3, result.Points.Single(point => point.Chainage == 1350).CurveElevation, 6);
    }

    [Fact]
    public void Calculate_ClassifiesCrestCurve()
    {
        var result = new VerticalCurveCalculator().Calculate(CreateInput(gradeInPercent: 2, gradeOutPercent: -1.5));

        Assert.Equal("Crest", result.CurveType);
    }

    [Fact]
    public void Calculate_ClassifiesSagCurve()
    {
        var result = new VerticalCurveCalculator().Calculate(CreateInput(gradeInPercent: -1, gradeOutPercent: 2));

        Assert.Equal("Sag", result.CurveType);
    }

    [Fact]
    public void Calculate_AddsWarningForInvalidCurveLength()
    {
        var result = new VerticalCurveCalculator().Calculate(CreateInput(curveLength: 0));

        Assert.Contains(result.Warnings, warning => warning.Contains("length", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(result.Points);
    }

    [Fact]
    public void ParseVerticalCurve_ParsesValidFile()
    {
        var parser = new ParseService();

        var result = parser.ParseVerticalCurve(
            """
            VERTICAL_CURVE VC1
            PVI_CHAINAGE 1250.000
            PVI_ELEVATION 56.800
            GRADE_IN 2.000
            GRADE_OUT -1.500
            LENGTH 200.000
            CHAINAGES
            1150.000
            1200.000
            1250.000
            1300.000
            1350.000
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("VC1", result.Input!.CurveName);
        Assert.Equal(1250, result.Input.PviChainage, 3);
        Assert.Equal(56.8, result.Input.PviElevation, 3);
        Assert.Equal(2, result.Input.GradeInPercent, 3);
        Assert.Equal(-1.5, result.Input.GradeOutPercent, 3);
        Assert.Equal(200, result.Input.CurveLength, 3);
        Assert.Equal(5, result.Input.DesignChainages.Count);
    }

    private static VerticalCurveInput CreateInput(
        double gradeInPercent = 2,
        double gradeOutPercent = -1.5,
        double curveLength = 200)
    {
        return new VerticalCurveInput(
            "VC1",
            1250,
            56.8,
            gradeInPercent,
            gradeOutPercent,
            curveLength,
            new List<double> { 1150, 1200, 1250, 1300, 1350 });
    }
}
