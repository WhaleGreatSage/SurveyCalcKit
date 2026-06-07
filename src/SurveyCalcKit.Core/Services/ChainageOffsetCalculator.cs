using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class ChainageOffsetCalculator
{
    private const double ZeroTolerance = 1e-12;
    private const double LineTolerance = 1e-9;
    private const double SmallOffsetTolerance = 0.001;

    public ChainageOffsetResult Calculate(ChainageOffsetInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var vx = input.EndX - input.StartX;
        var vy = input.EndY - input.StartY;
        var wx = input.TargetX - input.StartX;
        var wy = input.TargetY - input.StartY;
        var lengthSquared = vx * vx + vy * vy;

        if (lengthSquared <= ZeroTolerance)
        {
            warnings.Add("Baseline length is zero; chainage and side cannot be calculated reliably.");
            var offsetToStart = Math.Sqrt(wx * wx + wy * wy);
            return new ChainageOffsetResult(
                input.BaselineStartName,
                input.BaselineEndName,
                input.TargetPointName,
                0,
                0,
                input.StartChainage,
                offsetToStart,
                "Undefined",
                false,
                input.StartX,
                input.StartY,
                warnings);
        }

        var baselineLength = Math.Sqrt(lengthSquared);
        var projectionRatio = (wx * vx + wy * vy) / lengthSquared;
        var projectionX = input.StartX + projectionRatio * vx;
        var projectionY = input.StartY + projectionRatio * vy;
        var along = projectionRatio * baselineLength;
        var chainage = input.StartChainage + along;
        var offsetX = input.TargetX - projectionX;
        var offsetY = input.TargetY - projectionY;
        var offset = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
        var cross = vx * wy - vy * wx;
        var projectionInside = projectionRatio >= 0 && projectionRatio <= 1;
        var side = DetermineSide(cross, baselineLength);

        if (!projectionInside)
        {
            warnings.Add("Projection falls outside the baseline segment.");
        }

        if (side == "OnLine")
        {
            warnings.Add("Target point is on the baseline within tolerance.");
        }

        if (offset > 0 && offset < SmallOffsetTolerance)
        {
            warnings.Add("Offset is extremely small; target point is almost on the baseline.");
        }

        return new ChainageOffsetResult(
            input.BaselineStartName,
            input.BaselineEndName,
            input.TargetPointName,
            baselineLength,
            projectionRatio,
            chainage,
            offset,
            side,
            projectionInside,
            projectionX,
            projectionY,
            warnings);
    }

    private static string DetermineSide(double cross, double baselineLength)
    {
        var tolerance = LineTolerance * Math.Max(1, baselineLength);
        if (cross > tolerance)
        {
            return "Left";
        }

        if (cross < -tolerance)
        {
            return "Right";
        }

        return "OnLine";
    }
}
