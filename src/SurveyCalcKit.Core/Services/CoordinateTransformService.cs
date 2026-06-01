using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class CoordinateTransformService
{
    public IReadOnlyList<PointRecord> Transform(
        IEnumerable<PointRecord> points,
        double dx,
        double dy,
        double scale = 1.0,
        double rotationAngleDegrees = 0.0)
    {
        ArgumentNullException.ThrowIfNull(points);

        var radians = rotationAngleDegrees * Math.PI / 180.0;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);

        return points
            .Select(point =>
            {
                var rotatedX = point.X * cos - point.Y * sin;
                var rotatedY = point.X * sin + point.Y * cos;

                return new PointRecord(
                    point.Name,
                    rotatedX * scale + dx,
                    rotatedY * scale + dy,
                    point.H);
            })
            .ToList();
    }
}
