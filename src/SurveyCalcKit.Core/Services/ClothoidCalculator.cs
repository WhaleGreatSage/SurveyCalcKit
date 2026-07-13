using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class ClothoidCalculator
{
    public ClothoidResult Calculate(ClothoidInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        if (!double.IsFinite(input.Radius) || input.Radius <= 0)
        {
            warnings.Add("Radius must be greater than zero.");
        }

        if (!double.IsFinite(input.SpiralLength) || input.SpiralLength <= 0)
        {
            warnings.Add("Spiral length must be greater than zero.");
        }

        if (!AlignmentMath.TryGetDirectionSign(input.TurnDirection, out var directionSign))
        {
            warnings.Add("Turn direction must be LEFT or RIGHT.");
        }

        if (warnings.Count > 0)
        {
            return new ClothoidResult(
                input.CurveName,
                input.StartX,
                input.StartY,
                input.StartAzimuthDegrees,
                input.Radius,
                input.SpiralLength,
                0,
                0,
                0,
                input.TurnDirection,
                new List<ClothoidPointResult>(),
                warnings);
        }

        var start = new AlignmentState(0, input.StartX, input.StartY, input.StartAzimuthDegrees, 0, "START", "START");
        var element = new ClothoidAlignmentElement(input.CurveName, start, input.SpiralLength, 0, directionSign / input.Radius);
        var points = new List<ClothoidPointResult>();
        foreach (var requestedDistance in input.DistancesFromStart)
        {
            var isInside = requestedDistance >= 0 && requestedDistance <= input.SpiralLength;
            if (!isInside)
            {
                warnings.Add($"Distance {requestedDistance:0.###} is outside the spiral limits and was clamped.");
            }

            var state = element.GetStateAt(requestedDistance);
            points.Add(new ClothoidPointResult(
                requestedDistance,
                state.X,
                state.Y,
                state.AzimuthDegrees,
                state.Curvature,
                Math.Abs(state.Curvature) <= AlignmentMath.CurvatureTolerance ? double.PositiveInfinity : 1.0 / Math.Abs(state.Curvature),
                isInside));
        }

        return new ClothoidResult(
            input.CurveName,
            input.StartX,
            input.StartY,
            AlignmentMath.NormalizeAzimuth(input.StartAzimuthDegrees),
            input.Radius,
            input.SpiralLength,
            Math.Sqrt(input.Radius * input.SpiralLength),
            AlignmentMath.ToDegrees(input.SpiralLength / (2.0 * input.Radius)),
            input.SpiralLength * input.SpiralLength / (24.0 * input.Radius),
            directionSign > 0 ? "Left" : "Right",
            points,
            warnings);
    }
}
