using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class CircularCurveCalculator
{
    public CircularCurveResult Calculate(CircularCurveInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var direction = NormalizeDirection(input.TurnDirection, warnings);
        if (!double.IsFinite(input.Radius) || input.Radius <= 0)
        {
            warnings.Add("Radius must be greater than zero.");
        }

        if (!double.IsFinite(input.DeflectionAngleDegrees) ||
            input.DeflectionAngleDegrees <= 0 ||
            input.DeflectionAngleDegrees >= 180)
        {
            warnings.Add("Deflection angle must be greater than 0 and less than 180 degrees.");
        }

        if (warnings.Any(warning =>
                warning.Contains("Radius", StringComparison.OrdinalIgnoreCase) ||
                warning.Contains("Deflection angle", StringComparison.OrdinalIgnoreCase)))
        {
            return new CircularCurveResult(
                input.CurveName,
                input.PiChainage,
                input.Radius,
                input.DeflectionAngleDegrees,
                direction,
                0,
                0,
                0,
                0,
                input.PiChainage,
                input.PiChainage,
                warnings);
        }

        var halfAngleRadians = input.DeflectionAngleDegrees * Math.PI / 180.0 / 2.0;
        var tangentLength = input.Radius * Math.Tan(halfAngleRadians);
        var curveLength = Math.PI * input.Radius * input.DeflectionAngleDegrees / 180.0;
        var externalDistance = input.Radius * ((1.0 / Math.Cos(halfAngleRadians)) - 1.0);
        var middleOrdinate = input.Radius * (1.0 - Math.Cos(halfAngleRadians));
        var pcChainage = input.PiChainage - tangentLength;
        var ptChainage = pcChainage + curveLength;

        return new CircularCurveResult(
            input.CurveName,
            input.PiChainage,
            input.Radius,
            input.DeflectionAngleDegrees,
            direction,
            tangentLength,
            curveLength,
            externalDistance,
            middleOrdinate,
            pcChainage,
            ptChainage,
            warnings);
    }

    private static string NormalizeDirection(string direction, List<string> warnings)
    {
        if (string.Equals(direction, "Left", StringComparison.OrdinalIgnoreCase))
        {
            return "Left";
        }

        if (string.Equals(direction, "Right", StringComparison.OrdinalIgnoreCase))
        {
            return "Right";
        }

        warnings.Add("Turn direction should be Left or Right.");
        return string.IsNullOrWhiteSpace(direction) ? "Unknown" : direction;
    }
}
