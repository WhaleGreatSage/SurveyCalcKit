using System.Globalization;
using System.Text.RegularExpressions;
using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class ParseService
{
    public ParseResult ParsePoints(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var points = new List<PointRecord>();
        var errors = new List<ParseError>();

        using var reader = new StringReader(text);
        var lineNumber = 0;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var fields = SplitFields(trimmed);
            if (fields.Length is not (3 or 4))
            {
                errors.Add(new ParseError(
                    lineNumber,
                    line,
                    $"Line {lineNumber}: Expected 3 or 4 fields (Name X Y [H]), but found {fields.Length}."));
                continue;
            }

            var name = fields[0];
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Point name is required."));
                continue;
            }

            if (!TryParseNumber(fields[1], out var x))
            {
                errors.Add(CreateNumericError(lineNumber, line, "X", fields[1]));
                continue;
            }

            if (!TryParseNumber(fields[2], out var y))
            {
                errors.Add(CreateNumericError(lineNumber, line, "Y", fields[2]));
                continue;
            }

            double? h = null;
            if (fields.Length == 4)
            {
                if (!TryParseNumber(fields[3], out var parsedH))
                {
                    errors.Add(CreateNumericError(lineNumber, line, "H", fields[3]));
                    continue;
                }

                h = parsedH;
            }

            points.Add(new PointRecord(name, x, y, h));
        }

        return new ParseResult(points, errors);
    }

    public LevelingRouteParseResult ParseLevelingRoute(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string? startBenchmarkName = null;
        string? endBenchmarkName = null;
        double? startElevation = null;
        double? endElevation = null;
        var observations = new List<LevelingObservation>();
        var errors = new List<ParseError>();
        var hasEndLine = false;

        using var reader = new StringReader(text);
        var lineNumber = 0;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var fields = SplitFields(trimmed);
            if (fields.Length != 3)
            {
                errors.Add(new ParseError(
                    lineNumber,
                    line,
                    $"Line {lineNumber}: Expected 3 fields for START, END, or observation row."));
                continue;
            }

            var keyword = fields[0].ToUpperInvariant();
            if (keyword == "START")
            {
                if (startBenchmarkName is not null)
                {
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: START line appears more than once."));
                    continue;
                }

                if (!TryParseNumber(fields[2], out var parsedStartElevation))
                {
                    errors.Add(CreateNumericError(lineNumber, line, "Start elevation", fields[2]));
                    continue;
                }

                startBenchmarkName = fields[1];
                startElevation = parsedStartElevation;
                continue;
            }

            if (keyword == "END")
            {
                if (endBenchmarkName is not null)
                {
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: END line appears more than once."));
                    continue;
                }

                if (!TryParseNumber(fields[2], out var parsedEndElevation))
                {
                    errors.Add(CreateNumericError(lineNumber, line, "End elevation", fields[2]));
                    continue;
                }

                endBenchmarkName = fields[1];
                endElevation = parsedEndElevation;
                hasEndLine = true;
                continue;
            }

            if (startBenchmarkName is null)
            {
                errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Observation appears before START line."));
                continue;
            }

            if (hasEndLine)
            {
                errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Observation appears after END line."));
                continue;
            }

            if (!TryParseNumber(fields[1], out var backsight))
            {
                errors.Add(CreateNumericError(lineNumber, line, "Backsight", fields[1]));
                continue;
            }

            if (!TryParseNumber(fields[2], out var foresight))
            {
                errors.Add(CreateNumericError(lineNumber, line, "Foresight", fields[2]));
                continue;
            }

            observations.Add(new LevelingObservation(fields[0], backsight, foresight));
        }

        if (startBenchmarkName is null || !startElevation.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Leveling route requires a START line such as: START BM1 100.000."));
        }

        if (endBenchmarkName is null || !endElevation.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Leveling route requires an END line such as: END BM2 100.480."));
        }

        if (errors.Count > 0)
        {
            return new LevelingRouteParseResult(null, errors);
        }

        return new LevelingRouteParseResult(
            new LevelingRouteInput(
                startBenchmarkName!,
                startElevation!.Value,
                endBenchmarkName!,
                endElevation!.Value,
                observations),
            errors);
    }

    public CoordinateForwardParseResult ParseCoordinateForward(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string? startPointName = null;
        string? endPointName = null;
        double? startX = null;
        double? startY = null;
        double? azimuth = null;
        double? distance = null;
        var errors = new List<ParseError>();

        using var reader = new StringReader(text);
        var lineNumber = 0;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var fields = SplitFields(trimmed);
            var keyword = fields[0].ToUpperInvariant();
            switch (keyword)
            {
                case "START":
                    if (fields.Length != 4)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: START requires 4 fields: START name X Y."));
                        break;
                    }

                    if (!TryParseNumber(fields[2], out var parsedStartX))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Start X", fields[2]));
                        break;
                    }

                    if (!TryParseNumber(fields[3], out var parsedStartY))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Start Y", fields[3]));
                        break;
                    }

                    startPointName = fields[1];
                    startX = parsedStartX;
                    startY = parsedStartY;
                    break;
                case "AZIMUTH":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: AZIMUTH requires 2 fields: AZIMUTH degrees."));
                        break;
                    }

                    if (!TryParseNumber(fields[1], out var parsedAzimuth))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Azimuth", fields[1]));
                        break;
                    }

                    azimuth = parsedAzimuth;
                    break;
                case "DISTANCE":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: DISTANCE requires 2 fields: DISTANCE value."));
                        break;
                    }

                    if (!TryParseNumber(fields[1], out var parsedDistance))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Distance", fields[1]));
                        break;
                    }

                    distance = parsedDistance;
                    break;
                case "END":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: END requires 2 fields: END name."));
                        break;
                    }

                    endPointName = fields[1];
                    break;
                default:
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Unknown coordinate forward keyword '{fields[0]}'."));
                    break;
            }
        }

        if (startPointName is null || !startX.HasValue || !startY.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Coordinate forward input requires START name X Y."));
        }

        if (!azimuth.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Coordinate forward input requires AZIMUTH degrees."));
        }

        if (!distance.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Coordinate forward input requires DISTANCE value."));
        }

        if (endPointName is null)
        {
            errors.Add(new ParseError(0, string.Empty, "Coordinate forward input requires END name."));
        }

        if (errors.Count > 0)
        {
            return new CoordinateForwardParseResult(null, errors);
        }

        return new CoordinateForwardParseResult(
            new CoordinateForwardInput(
                startPointName!,
                startX!.Value,
                startY!.Value,
                azimuth!.Value,
                distance!.Value,
                endPointName!),
            errors);
    }

    public ChainageOffsetParseResult ParseChainageOffset(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string? baselineStartName = null;
        string? baselineEndName = null;
        string? targetPointName = null;
        double? startX = null;
        double? startY = null;
        double? endX = null;
        double? endY = null;
        double? targetX = null;
        double? targetY = null;
        var startChainage = 0.0;
        var errors = new List<ParseError>();

        using var reader = new StringReader(text);
        var lineNumber = 0;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var fields = SplitFields(trimmed);
            var keyword = fields[0].ToUpperInvariant();
            switch (keyword)
            {
                case "BASELINE":
                    if (fields.Length != 7)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: BASELINE requires 7 fields: BASELINE A X1 Y1 B X2 Y2."));
                        break;
                    }

                    if (!TryParseNumber(fields[2], out var parsedStartX))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Baseline start X", fields[2]));
                        break;
                    }

                    if (!TryParseNumber(fields[3], out var parsedStartY))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Baseline start Y", fields[3]));
                        break;
                    }

                    if (!TryParseNumber(fields[5], out var parsedEndX))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Baseline end X", fields[5]));
                        break;
                    }

                    if (!TryParseNumber(fields[6], out var parsedEndY))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Baseline end Y", fields[6]));
                        break;
                    }

                    baselineStartName = fields[1];
                    baselineEndName = fields[4];
                    startX = parsedStartX;
                    startY = parsedStartY;
                    endX = parsedEndX;
                    endY = parsedEndY;
                    break;
                case "START_CHAINAGE":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: START_CHAINAGE requires 2 fields: START_CHAINAGE value."));
                        break;
                    }

                    if (!TryParseNumber(fields[1], out var parsedStartChainage))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Start chainage", fields[1]));
                        break;
                    }

                    startChainage = parsedStartChainage;
                    break;
                case "POINT":
                    if (fields.Length != 4)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: POINT requires 4 fields: POINT name X Y."));
                        break;
                    }

                    if (!TryParseNumber(fields[2], out var parsedTargetX))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Target X", fields[2]));
                        break;
                    }

                    if (!TryParseNumber(fields[3], out var parsedTargetY))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Target Y", fields[3]));
                        break;
                    }

                    targetPointName = fields[1];
                    targetX = parsedTargetX;
                    targetY = parsedTargetY;
                    break;
                default:
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Unknown chainage/offset keyword '{fields[0]}'."));
                    break;
            }
        }

        if (baselineStartName is null || baselineEndName is null || !startX.HasValue || !startY.HasValue || !endX.HasValue || !endY.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Chainage/offset input requires BASELINE A X1 Y1 B X2 Y2."));
        }

        if (targetPointName is null || !targetX.HasValue || !targetY.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Chainage/offset input requires POINT name X Y."));
        }

        if (errors.Count > 0)
        {
            return new ChainageOffsetParseResult(null, errors);
        }

        return new ChainageOffsetParseResult(
            new ChainageOffsetInput(
                baselineStartName!,
                startX!.Value,
                startY!.Value,
                baselineEndName!,
                endX!.Value,
                endY!.Value,
                targetPointName!,
                targetX!.Value,
                targetY!.Value,
                startChainage),
            errors);
    }

    private static string[] SplitFields(string line)
    {
        if (line.Contains(','))
        {
            return line.Split(',', StringSplitOptions.TrimEntries);
        }

        return Regex.Split(line, @"\s+");
    }

    private static bool TryParseNumber(string value, out double result)
    {
        return double.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out result);
    }

    private static ParseError CreateNumericError(int lineNumber, string rawLine, string fieldName, string value)
    {
        return new ParseError(
            lineNumber,
            rawLine,
            $"Line {lineNumber}: Invalid {fieldName} value '{value}'. Use a decimal number such as 100.000.");
    }
}
