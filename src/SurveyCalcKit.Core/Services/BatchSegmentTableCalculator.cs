using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class BatchSegmentTableCalculator
{
    private const double ZeroTolerance = 1e-12;
    private readonly TraverseCalculator traverseCalculator = new();

    public BatchSegmentTableResult Calculate(IEnumerable<PointRecord> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var pointList = points.ToList();
        var warnings = new List<string>();
        var rows = new List<BatchSegmentRow>();

        if (pointList.Count < 2)
        {
            warnings.Add("Batch segment table requires at least two points.");
            return new BatchSegmentTableResult(pointList.Count, 0, 0, rows, warnings);
        }

        var segments = traverseCalculator.CalculateSegments(pointList);
        var cumulative = 0.0;
        var index = 1;
        foreach (var segment in segments)
        {
            cumulative += segment.Distance2D;
            rows.Add(new BatchSegmentRow(
                index,
                segment.From,
                segment.To,
                segment.Dx,
                segment.Dy,
                segment.Distance2D,
                segment.AzimuthDegrees,
                cumulative,
                segment.DeltaH,
                segment.SlopePercent));

            if (segment.Distance2D <= ZeroTolerance)
            {
                warnings.Add($"Segment {segment.From}->{segment.To} has repeated consecutive coordinates.");
            }

            index++;
        }

        return new BatchSegmentTableResult(
            pointList.Count,
            rows.Count,
            cumulative,
            rows,
            warnings);
    }
}
