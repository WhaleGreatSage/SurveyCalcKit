using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class CoordinateInverseCalculator
{
    private const double ZeroTolerance = 1e-12;
    private readonly TraverseCalculator traverseCalculator = new();

    public CoordinateInverseResult Calculate(CoordinateInverseInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var dx = input.ToX - input.FromX;
        var dy = input.ToY - input.FromY;
        var distance2D = Math.Sqrt(dx * dx + dy * dy);
        var azimuth = traverseCalculator.CalculateAzimuthDegrees(dx, dy);

        if (distance2D <= ZeroTolerance)
        {
            warnings.Add("From and To points are identical or extremely close; azimuth is reported as 0.");
        }

        double? deltaH = null;
        double? distance3D = null;
        if (input.FromH.HasValue && input.ToH.HasValue)
        {
            deltaH = input.ToH.Value - input.FromH.Value;
            distance3D = Math.Sqrt(distance2D * distance2D + deltaH.Value * deltaH.Value);
        }
        else if (input.FromH.HasValue || input.ToH.HasValue)
        {
            warnings.Add("Only one point has elevation; delta H and 3D distance were not calculated.");
        }

        return new CoordinateInverseResult(
            input.FromPointName,
            input.ToPointName,
            dx,
            dy,
            distance2D,
            azimuth,
            deltaH,
            distance3D,
            warnings);
    }
}
