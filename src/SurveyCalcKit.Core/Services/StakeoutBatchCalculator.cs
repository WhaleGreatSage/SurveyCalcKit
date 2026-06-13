using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class StakeoutBatchCalculator
{
    private const double OffsetTolerance = 1e-9;
    private const double LargeOffsetWarningThreshold = 1000.0;

    public StakeoutBatchResult Calculate(StakeoutBatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var azimuth = NormalizeDegrees(input.BaselineAzimuthDegrees);
        if (!double.IsFinite(input.BaselineAzimuthDegrees))
        {
            warnings.Add("Azimuth is not a finite number; 0 degrees was used.");
            azimuth = 0;
        }

        if (input.Records.Count == 0)
        {
            warnings.Add("No stakeout records were provided.");
        }

        var duplicateNames = input.Records
            .GroupBy(record => record.PointName, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
        foreach (var name in duplicateNames)
        {
            warnings.Add($"Duplicate stakeout point name: {name}.");
        }

        var radians = azimuth * Math.PI / 180.0;
        var ux = Math.Cos(radians);
        var uy = Math.Sin(radians);
        var leftX = -uy;
        var leftY = ux;
        var points = new List<StakeoutPointResult>();

        foreach (var record in input.Records)
        {
            if (Math.Abs(record.Offset) > LargeOffsetWarningThreshold)
            {
                warnings.Add($"Point {record.PointName} has a very large offset.");
            }

            var alongDistance = record.Chainage - input.StartChainage;
            var x = input.OriginX + alongDistance * ux + record.Offset * leftX;
            var y = input.OriginY + alongDistance * uy + record.Offset * leftY;
            points.Add(new StakeoutPointResult(
                record.PointName,
                record.Chainage,
                record.Offset,
                x,
                y,
                DetermineSide(record.Offset)));
        }

        return new StakeoutBatchResult(
            input.OriginPointName,
            input.OriginX,
            input.OriginY,
            azimuth,
            input.StartChainage,
            points,
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

    private static string DetermineSide(double offset)
    {
        if (offset > OffsetTolerance)
        {
            return "Left";
        }

        if (offset < -OffsetTolerance)
        {
            return "Right";
        }

        return "OnLine";
    }
}
