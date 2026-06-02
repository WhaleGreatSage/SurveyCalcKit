using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class ClosedTraverseCalculator
{
    private const double CoordinateTolerance = 0.001;
    private const double ZeroTolerance = 1e-12;

    public TraverseClosureResult Calculate(IEnumerable<PointRecord> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var pointList = points.ToList();
        var warnings = new List<string>();

        if (pointList.Count == 0)
        {
            warnings.Add("Closed traverse calculation requires at least 4 points.");
            return CreateEmptyResult(warnings);
        }

        var start = pointList[0];
        var end = pointList[^1];
        var fx = end.X - start.X;
        var fy = end.Y - start.Y;
        var closureError = Math.Sqrt(fx * fx + fy * fy);

        if (pointList.Count < 4)
        {
            warnings.Add("Closed traverse calculation requires at least 4 points.");
            return CreateResultWithoutAdjustment(start, end, fx, fy, closureError, 0, warnings);
        }

        var segmentLengths = CalculateSegmentLengths(pointList);
        var totalLength = segmentLengths.Sum();
        var relativeClosureRatio = CalculateRelativeClosureRatio(totalLength, closureError);

        if (!IsClosedByName(start, end) && closureError > CoordinateTolerance)
        {
            warnings.Add("Input is not clearly closed: first and last point names differ and coordinates are not within tolerance.");
        }

        if (closureError <= ZeroTolerance)
        {
            warnings.Add("Perfect closure: coordinate closure error is zero.");
        }

        if (totalLength <= ZeroTolerance)
        {
            warnings.Add("Cannot apply Bowditch adjustment because total length is zero.");
            return CreateResultWithoutAdjustment(start, end, fx, fy, closureError, totalLength, warnings);
        }

        var adjustedSegments = BuildAdjustedSegments(pointList, segmentLengths, fx, fy, totalLength);
        var adjustedPoints = BuildAdjustedPoints(pointList, adjustedSegments, start);

        return new TraverseClosureResult(
            start.Name,
            end.Name,
            start.X,
            start.Y,
            end.X,
            end.Y,
            fx,
            fy,
            closureError,
            totalLength,
            relativeClosureRatio,
            adjustedPoints,
            adjustedSegments,
            warnings);
    }

    private static IReadOnlyList<double> CalculateSegmentLengths(IReadOnlyList<PointRecord> points)
    {
        var lengths = new List<double>(Math.Max(0, points.Count - 1));
        for (var i = 0; i < points.Count - 1; i++)
        {
            var dx = points[i + 1].X - points[i].X;
            var dy = points[i + 1].Y - points[i].Y;
            lengths.Add(Math.Sqrt(dx * dx + dy * dy));
        }

        return lengths;
    }

    private static IReadOnlyList<AdjustedSegmentResult> BuildAdjustedSegments(
        IReadOnlyList<PointRecord> points,
        IReadOnlyList<double> segmentLengths,
        double fx,
        double fy,
        double totalLength)
    {
        var segments = new List<AdjustedSegmentResult>(segmentLengths.Count);
        for (var i = 0; i < segmentLengths.Count; i++)
        {
            var from = points[i];
            var to = points[i + 1];
            var originalDx = to.X - from.X;
            var originalDy = to.Y - from.Y;
            var correctionDx = -fx * segmentLengths[i] / totalLength;
            var correctionDy = -fy * segmentLengths[i] / totalLength;

            segments.Add(new AdjustedSegmentResult(
                from.Name,
                to.Name,
                originalDx,
                originalDy,
                segmentLengths[i],
                correctionDx,
                correctionDy,
                originalDx + correctionDx,
                originalDy + correctionDy));
        }

        return segments;
    }

    private static IReadOnlyList<AdjustedPointRecord> BuildAdjustedPoints(
        IReadOnlyList<PointRecord> points,
        IReadOnlyList<AdjustedSegmentResult> adjustedSegments,
        PointRecord start)
    {
        var adjustedPoints = new List<AdjustedPointRecord>(points.Count)
        {
            new(start.Name, start.X, start.Y, start.X, start.Y, 0, 0, start.H)
        };

        var currentX = start.X;
        var currentY = start.Y;
        for (var i = 0; i < adjustedSegments.Count; i++)
        {
            currentX += adjustedSegments[i].AdjustedDx;
            currentY += adjustedSegments[i].AdjustedDy;
            var originalPoint = points[i + 1];

            adjustedPoints.Add(new AdjustedPointRecord(
                originalPoint.Name,
                originalPoint.X,
                originalPoint.Y,
                currentX,
                currentY,
                currentX - originalPoint.X,
                currentY - originalPoint.Y,
                originalPoint.H));
        }

        return adjustedPoints;
    }

    private static TraverseClosureResult CreateEmptyResult(IReadOnlyList<string> warnings)
    {
        return new TraverseClosureResult(
            string.Empty,
            string.Empty,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            double.PositiveInfinity,
            Array.Empty<AdjustedPointRecord>(),
            Array.Empty<AdjustedSegmentResult>(),
            warnings);
    }

    private static TraverseClosureResult CreateResultWithoutAdjustment(
        PointRecord start,
        PointRecord end,
        double fx,
        double fy,
        double closureError,
        double totalLength,
        IReadOnlyList<string> warnings)
    {
        return new TraverseClosureResult(
            start.Name,
            end.Name,
            start.X,
            start.Y,
            end.X,
            end.Y,
            fx,
            fy,
            closureError,
            totalLength,
            CalculateRelativeClosureRatio(totalLength, closureError),
            Array.Empty<AdjustedPointRecord>(),
            Array.Empty<AdjustedSegmentResult>(),
            warnings);
    }

    private static double CalculateRelativeClosureRatio(double totalLength, double closureError)
    {
        return closureError <= ZeroTolerance
            ? double.PositiveInfinity
            : totalLength / closureError;
    }

    private static bool IsClosedByName(PointRecord start, PointRecord end)
    {
        return string.Equals(start.Name, end.Name, StringComparison.OrdinalIgnoreCase);
    }
}
