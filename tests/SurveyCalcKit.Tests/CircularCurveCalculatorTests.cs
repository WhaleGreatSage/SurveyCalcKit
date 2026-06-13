using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class CircularCurveCalculatorTests
{
    [Fact]
    public void Calculate_ComputesValidCircularCurve()
    {
        var calculator = new CircularCurveCalculator();

        var result = calculator.Calculate(CreateInput());

        Assert.Equal("C1", result.CurveName);
        Assert.Equal(1250, result.PiChainage, 6);
        Assert.Equal(300, result.Radius, 6);
        Assert.Equal(42.5, result.DeflectionAngleDegrees, 6);
        Assert.Equal("Right", result.TurnDirection);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Calculate_ComputesTangentLength()
    {
        var result = new CircularCurveCalculator().Calculate(CreateInput());

        Assert.Equal(300 * Math.Tan(42.5 * Math.PI / 180 / 2), result.TangentLength, 6);
    }

    [Fact]
    public void Calculate_ComputesCurveLength()
    {
        var result = new CircularCurveCalculator().Calculate(CreateInput());

        Assert.Equal(Math.PI * 300 * 42.5 / 180, result.CurveLength, 6);
    }

    [Fact]
    public void Calculate_ComputesPcAndPtChainage()
    {
        var result = new CircularCurveCalculator().Calculate(CreateInput());

        Assert.Equal(result.PiChainage - result.TangentLength, result.PcChainage, 6);
        Assert.Equal(result.PcChainage + result.CurveLength, result.PtChainage, 6);
    }

    [Fact]
    public void Calculate_AddsWarningForInvalidRadius()
    {
        var input = CreateInput(radius: 0);

        var result = new CircularCurveCalculator().Calculate(input);

        Assert.Contains(result.Warnings, warning => warning.Contains("radius", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_AddsWarningForInvalidAngle()
    {
        var input = CreateInput(deflectionAngleDegrees: 180);

        var result = new CircularCurveCalculator().Calculate(input);

        Assert.Contains(result.Warnings, warning => warning.Contains("angle", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseCircularCurve_ParsesValidFile()
    {
        var parser = new ParseService();

        var result = parser.ParseCircularCurve(
            """
            CURVE C1
            PI_CHAINAGE 1250.000
            RADIUS 300.000
            ANGLE 42.5000
            DIRECTION RIGHT
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("C1", result.Input!.CurveName);
        Assert.Equal(1250, result.Input.PiChainage, 3);
        Assert.Equal(300, result.Input.Radius, 3);
        Assert.Equal(42.5, result.Input.DeflectionAngleDegrees, 4);
        Assert.Equal("Right", result.Input.TurnDirection);
    }

    private static CircularCurveInput CreateInput(double radius = 300, double deflectionAngleDegrees = 42.5)
    {
        return new CircularCurveInput("C1", 1250, radius, deflectionAngleDegrees, "Right");
    }
}
