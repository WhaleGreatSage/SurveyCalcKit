using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class CoordinateForwardCalculator
{
    public CoordinateForwardResult Calculate(CoordinateForwardInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var azimuth = NormalizeDegrees(input.AzimuthDegrees);
        var distance = input.Distance;

        if (!double.IsFinite(input.AzimuthDegrees))
        {
            warnings.Add("Azimuth is not a finite number; 0 degrees was used.");
            azimuth = 0;
        }

        if (input.Distance < 0)
        {
            warnings.Add("Distance is negative; 0 was used to avoid reversing the direction silently.");
            distance = 0;
        }

        if (!double.IsFinite(distance))
        {
            warnings.Add("Distance is not a finite number; 0 was used.");
            distance = 0;
        }

        if (string.IsNullOrWhiteSpace(input.StartPointName))
        {
            warnings.Add("Start point name is empty.");
        }

        if (string.IsNullOrWhiteSpace(input.EndPointName))
        {
            warnings.Add("End point name is empty.");
        }

        var radians = azimuth * Math.PI / 180.0;
        var deltaX = distance * Math.Cos(radians);
        var deltaY = distance * Math.Sin(radians);
        var endX = input.StartX + deltaX;
        var endY = input.StartY + deltaY;

        return new CoordinateForwardResult(
            input.StartPointName,
            input.StartX,
            input.StartY,
            azimuth,
            distance,
            deltaX,
            deltaY,
            input.EndPointName,
            endX,
            endY,
            warnings);
    }

    private static double NormalizeDegrees(double degrees)
    {
        if (!double.IsFinite(degrees))
        {
            return 0;
        }

        var normalized = degrees % 360.0;
        return normalized < 0 ? normalized + 360.0 : normalized;
    }
}
