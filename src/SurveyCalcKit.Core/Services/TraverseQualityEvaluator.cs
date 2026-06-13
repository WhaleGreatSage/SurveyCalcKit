using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class TraverseQualityEvaluator
{
    private const double ClosureTolerance = 1e-9;
    private readonly TraverseCalculator traverseCalculator = new();

    public TraverseQualityResult Evaluate(TraverseQualityInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var points = input.Points ?? new List<PointRecord>();
        var warnings = new List<string>();
        var segments = traverseCalculator.CalculateSegments(points).ToList();
        var rows = BuildRows(segments);
        var totalLength = traverseCalculator.CalculateTotal2DLength(segments);
        var segmentCount = segments.Count;
        var start = points.FirstOrDefault();
        var end = points.LastOrDefault();
        var fx = start is null || end is null ? 0 : end.X - start.X;
        var fy = start is null || end is null ? 0 : end.Y - start.Y;
        var linearClosureError = Math.Sqrt(fx * fx + fy * fy);

        if (points.Count < 4)
        {
            warnings.Add("Closed traverse quality evaluation requires at least 4 points, including the closing point.");
        }

        if (points.Count >= 2 && !IsClosed(points[0], points[^1]))
        {
            warnings.Add("Traverse is not explicitly closed by matching start/end name or coordinates.");
        }

        if (totalLength <= ClosureTolerance)
        {
            warnings.Add("Total traverse length is zero; relative closure precision cannot be evaluated.");
        }

        var relativeClosureDenominator = 0.0;
        if (linearClosureError <= ClosureTolerance)
        {
            relativeClosureDenominator = double.PositiveInfinity;
            warnings.Add("Perfect closure: coordinate closure error is zero within tolerance.");
        }
        else if (totalLength > ClosureTolerance)
        {
            relativeClosureDenominator = totalLength / linearClosureError;
        }

        bool? passesLinear = null;
        if (input.AllowableRelativeClosureDenominator.HasValue)
        {
            passesLinear = relativeClosureDenominator >= input.AllowableRelativeClosureDenominator.Value;
            if (passesLinear == false)
            {
                warnings.Add("Linear relative closure precision is weaker than the allowable limit.");
            }
        }
        else
        {
            warnings.Add("Allowable relative closure denominator is missing; linear pass/fail was not evaluated.");
        }

        var stationCount = Math.Max(0, points.Count - 1);
        double? angularClosureErrorSeconds = null;
        double? allowableAngularClosureSeconds = null;
        bool? passesAngular = null;

        if (input.ObservedAnglesDegrees is { Count: > 0 } angles)
        {
            if (stationCount >= 3)
            {
                if (angles.Count != stationCount)
                {
                    warnings.Add($"Angular observation count is {angles.Count}, but {stationCount} angles are expected for this traverse.");
                }

                var theoreticalSum = (stationCount - 2) * 180.0;
                angularClosureErrorSeconds = (angles.Sum() - theoreticalSum) * 3600.0;

                if (input.AllowableAngularClosureSecondsPerStation.HasValue)
                {
                    allowableAngularClosureSeconds = input.AllowableAngularClosureSecondsPerStation.Value * Math.Sqrt(stationCount);
                    passesAngular = Math.Abs(angularClosureErrorSeconds.Value) <= allowableAngularClosureSeconds.Value;
                    if (passesAngular == false)
                    {
                        warnings.Add("Angular closure failed the allowable angular closure limit.");
                    }
                }
                else
                {
                    warnings.Add("Allowable angular closure seconds per station is missing; angular pass/fail was not evaluated.");
                }
            }
            else
            {
                warnings.Add("At least 3 traverse sides are required for angular closure evaluation.");
            }
        }
        else
        {
            warnings.Add("Angular observations are missing; angular closure was not evaluated.");
        }

        var qualityGrade = DetermineQualityGrade(
            points.Count,
            totalLength,
            relativeClosureDenominator,
            passesLinear,
            passesAngular);

        return new TraverseQualityResult(
            points.Count,
            segmentCount,
            totalLength,
            fx,
            fy,
            linearClosureError,
            relativeClosureDenominator,
            passesLinear,
            angularClosureErrorSeconds,
            allowableAngularClosureSeconds,
            passesAngular,
            qualityGrade,
            warnings,
            rows);
    }

    private static List<TraverseQualitySegmentRow> BuildRows(IReadOnlyList<SegmentResult> segments)
    {
        var rows = new List<TraverseQualitySegmentRow>();
        var cumulative = 0.0;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            cumulative += segment.Distance2D;
            rows.Add(new TraverseQualitySegmentRow(
                i + 1,
                segment.From,
                segment.To,
                segment.Dx,
                segment.Dy,
                segment.Distance2D,
                segment.AzimuthDegrees,
                cumulative));
        }

        return rows;
    }

    private static bool IsClosed(PointRecord start, PointRecord end)
    {
        return string.Equals(start.Name, end.Name, StringComparison.OrdinalIgnoreCase)
            || (Math.Abs(end.X - start.X) <= 0.001 && Math.Abs(end.Y - start.Y) <= 0.001);
    }

    private static string DetermineQualityGrade(
        int pointCount,
        double totalLength,
        double relativeClosureDenominator,
        bool? passesLinear,
        bool? passesAngular)
    {
        if (pointCount < 4 || totalLength <= ClosureTolerance)
        {
            return "NotEvaluated";
        }

        if (passesLinear == false || passesAngular == false)
        {
            return "Failed";
        }

        if (passesLinear is null && passesAngular is null)
        {
            return "NotEvaluated";
        }

        if (double.IsPositiveInfinity(relativeClosureDenominator) || relativeClosureDenominator >= 10000)
        {
            return "Excellent";
        }

        if (relativeClosureDenominator >= 5000)
        {
            return "Good";
        }

        return "Pass";
    }
}
