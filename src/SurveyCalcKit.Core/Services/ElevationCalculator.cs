using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class ElevationCalculator
{
    private const double ZeroTolerance = 1e-12;

    public double? CalculateDeltaH(PointRecord from, PointRecord to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (!from.H.HasValue || !to.H.HasValue)
        {
            return null;
        }

        return to.H.Value - from.H.Value;
    }

    public double? CalculateSlopePercent(double? deltaH, double horizontalDistance)
    {
        if (!deltaH.HasValue || Math.Abs(horizontalDistance) < ZeroTolerance)
        {
            return null;
        }

        return deltaH.Value / horizontalDistance * 100.0;
    }

    public double CalculateElevationClosureError(
        IEnumerable<PointRecord> points,
        double knownStartElevation,
        double knownEndElevation)
    {
        ArgumentNullException.ThrowIfNull(points);

        var pointList = points.ToList();
        if (pointList.Count < 2)
        {
            throw new ArgumentException("At least two points are required to calculate elevation closure error.", nameof(points));
        }

        var first = pointList.First();
        var last = pointList.Last();
        if (!first.H.HasValue || !last.H.HasValue)
        {
            throw new ArgumentException("The first and last point must both include elevation values.", nameof(points));
        }

        var observedDeltaH = last.H.Value - first.H.Value;
        var computedEndElevation = knownStartElevation + observedDeltaH;
        return computedEndElevation - knownEndElevation;
    }
}
