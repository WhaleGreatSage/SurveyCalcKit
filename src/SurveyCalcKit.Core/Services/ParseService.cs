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

    public CoordinateInverseParseResult ParseCoordinateInverse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string? fromPointName = null;
        string? toPointName = null;
        double? fromX = null;
        double? fromY = null;
        double? fromH = null;
        double? toX = null;
        double? toY = null;
        double? toH = null;
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
            if (fields.Length is not (4 or 5))
            {
                errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Expected 4 or 5 fields: FROM/TO name X Y [H]."));
                continue;
            }

            var keyword = fields[0].ToUpperInvariant();
            if (keyword is not ("FROM" or "TO"))
            {
                errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Expected FROM or TO, but found '{fields[0]}'."));
                continue;
            }

            if (!TryParseNumber(fields[2], out var x))
            {
                errors.Add(CreateNumericError(lineNumber, line, $"{keyword} X", fields[2]));
                continue;
            }

            if (!TryParseNumber(fields[3], out var y))
            {
                errors.Add(CreateNumericError(lineNumber, line, $"{keyword} Y", fields[3]));
                continue;
            }

            double? h = null;
            if (fields.Length == 5)
            {
                if (!TryParseNumber(fields[4], out var parsedH))
                {
                    errors.Add(CreateNumericError(lineNumber, line, $"{keyword} H", fields[4]));
                    continue;
                }

                h = parsedH;
            }

            if (keyword == "FROM")
            {
                fromPointName = fields[1];
                fromX = x;
                fromY = y;
                fromH = h;
            }
            else
            {
                toPointName = fields[1];
                toX = x;
                toY = y;
                toH = h;
            }
        }

        if (fromPointName is null || !fromX.HasValue || !fromY.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Coordinate inverse input requires FROM name X Y [H]."));
        }

        if (toPointName is null || !toX.HasValue || !toY.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Coordinate inverse input requires TO name X Y [H]."));
        }

        if (errors.Count > 0)
        {
            return new CoordinateInverseParseResult(null, errors);
        }

        return new CoordinateInverseParseResult(
            new CoordinateInverseInput(
                fromPointName!,
                fromX!.Value,
                fromY!.Value,
                fromH,
                toPointName!,
                toX!.Value,
                toY!.Value,
                toH),
            errors);
    }

    public TraverseQualityParseResult ParseTraverseQuality(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var points = new List<PointRecord>();
        var angles = new List<double>();
        double? allowableRelative = null;
        double? allowableAngularSecondsPerStation = null;
        string? section = null;
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
            if (keyword is "POINTS" or "ANGLES" or "LIMITS")
            {
                section = keyword;
                continue;
            }

            switch (section)
            {
                case "POINTS":
                    ParseTraverseQualityPoint(fields, lineNumber, line, points, errors);
                    break;
                case "ANGLES":
                    if (fields.Length != 1)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: ANGLES rows require one angle value."));
                        break;
                    }

                    if (!TryParseNumber(fields[0], out var angle))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Angle", fields[0]));
                        break;
                    }

                    angles.Add(angle);
                    break;
                case "LIMITS":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: LIMITS rows require key and value."));
                        break;
                    }

                    if (!TryParseNumber(fields[1], out var limitValue))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, fields[0], fields[1]));
                        break;
                    }

                    var limitKey = fields[0].ToUpperInvariant();
                    if (limitKey == "RELATIVE")
                    {
                        allowableRelative = limitValue;
                    }
                    else if (limitKey == "ANGULAR_SECONDS_PER_STATION")
                    {
                        allowableAngularSecondsPerStation = limitValue;
                    }
                    else
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Unknown LIMITS key '{fields[0]}'." ));
                    }

                    break;
                default:
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Expected POINTS, ANGLES, or LIMITS section before data rows."));
                    break;
            }
        }

        if (points.Count == 0)
        {
            errors.Add(new ParseError(0, string.Empty, "Traverse quality input requires a POINTS section."));
        }

        if (errors.Count > 0)
        {
            return new TraverseQualityParseResult(null, errors);
        }

        return new TraverseQualityParseResult(
            new TraverseQualityInput(
                points,
                angles.Count > 0 ? angles : null,
                allowableRelative,
                allowableAngularSecondsPerStation,
                "Closed"),
            errors);
    }

    public CircularCurveParseResult ParseCircularCurve(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string? curveName = null;
        double? piChainage = null;
        double? radius = null;
        double? angle = null;
        string? direction = null;
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
            if (fields.Length != 2)
            {
                errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Circular curve rows require key and value."));
                continue;
            }

            switch (fields[0].ToUpperInvariant())
            {
                case "CURVE":
                    curveName = fields[1];
                    break;
                case "PI_CHAINAGE":
                    if (!TryParseNumber(fields[1], out var parsedPi))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "PI chainage", fields[1]));
                        break;
                    }

                    piChainage = parsedPi;
                    break;
                case "RADIUS":
                    if (!TryParseNumber(fields[1], out var parsedRadius))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Radius", fields[1]));
                        break;
                    }

                    radius = parsedRadius;
                    break;
                case "ANGLE":
                    if (!TryParseNumber(fields[1], out var parsedAngle))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Angle", fields[1]));
                        break;
                    }

                    angle = parsedAngle;
                    break;
                case "DIRECTION":
                    direction = NormalizeDirectionText(fields[1]);
                    break;
                default:
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Unknown circular curve keyword '{fields[0]}'."));
                    break;
            }
        }

        if (curveName is null)
        {
            errors.Add(new ParseError(0, string.Empty, "Circular curve input requires CURVE name."));
        }

        if (!piChainage.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Circular curve input requires PI_CHAINAGE value."));
        }

        if (!radius.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Circular curve input requires RADIUS value."));
        }

        if (!angle.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Circular curve input requires ANGLE value."));
        }

        if (direction is null)
        {
            errors.Add(new ParseError(0, string.Empty, "Circular curve input requires DIRECTION Left or Right."));
        }

        if (errors.Count > 0)
        {
            return new CircularCurveParseResult(null, errors);
        }

        return new CircularCurveParseResult(
            new CircularCurveInput(curveName!, piChainage!.Value, radius!.Value, angle!.Value, direction!),
            errors);
    }

    public StakeoutBatchParseResult ParseStakeoutBatch(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string? originPointName = null;
        double? originX = null;
        double? originY = null;
        double? azimuth = null;
        var startChainage = 0.0;
        var records = new List<StakeoutRecord>();
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
                case "ORIGIN":
                    if (fields.Length != 4)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: ORIGIN requires 4 fields: ORIGIN name X Y."));
                        break;
                    }

                    if (!TryParseNumber(fields[2], out var parsedOriginX))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Origin X", fields[2]));
                        break;
                    }

                    if (!TryParseNumber(fields[3], out var parsedOriginY))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Origin Y", fields[3]));
                        break;
                    }

                    originPointName = fields[1];
                    originX = parsedOriginX;
                    originY = parsedOriginY;
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
                case "START_CHAINAGE":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: START_CHAINAGE requires 2 fields."));
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
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: POINT requires 4 fields: POINT name chainage offset."));
                        break;
                    }

                    if (!TryParseNumber(fields[2], out var chainage))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Chainage", fields[2]));
                        break;
                    }

                    if (!TryParseNumber(fields[3], out var offset))
                    {
                        errors.Add(CreateNumericError(lineNumber, line, "Offset", fields[3]));
                        break;
                    }

                    records.Add(new StakeoutRecord(fields[1], chainage, offset));
                    break;
                default:
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Unknown stakeout keyword '{fields[0]}'."));
                    break;
            }
        }

        if (originPointName is null || !originX.HasValue || !originY.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Stakeout input requires ORIGIN name X Y."));
        }

        if (!azimuth.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Stakeout input requires AZIMUTH degrees."));
        }

        if (records.Count == 0)
        {
            errors.Add(new ParseError(0, string.Empty, "Stakeout input requires at least one POINT row."));
        }

        if (errors.Count > 0)
        {
            return new StakeoutBatchParseResult(null, errors);
        }

        return new StakeoutBatchParseResult(
            new StakeoutBatchInput(originPointName!, originX!.Value, originY!.Value, azimuth!.Value, startChainage, records),
            errors);
    }

    public VerticalCurveParseResult ParseVerticalCurve(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string? curveName = null;
        double? pviChainage = null;
        double? pviElevation = null;
        double? gradeIn = null;
        double? gradeOut = null;
        double? curveLength = null;
        var chainages = new List<double>();
        var errors = new List<ParseError>();
        var readingChainages = false;

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
            if (readingChainages && fields.Length == 1)
            {
                if (!TryParseNumber(fields[0], out var chainageValue))
                {
                    errors.Add(CreateNumericError(lineNumber, line, "Design chainage", fields[0]));
                    continue;
                }

                chainages.Add(chainageValue);
                continue;
            }

            var keyword = fields[0].ToUpperInvariant();
            if (keyword != "CHAINAGE")
            {
                readingChainages = false;
            }

            switch (keyword)
            {
                case "VERTICAL_CURVE":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: VERTICAL_CURVE requires a curve name."));
                        break;
                    }

                    curveName = fields[1];
                    break;
                case "PVI_CHAINAGE":
                    pviChainage = ParseRequiredNumber(fields, lineNumber, line, "PVI chainage", errors);
                    break;
                case "PVI_ELEVATION":
                    pviElevation = ParseRequiredNumber(fields, lineNumber, line, "PVI elevation", errors);
                    break;
                case "GRADE_IN":
                    gradeIn = ParseRequiredNumber(fields, lineNumber, line, "Grade in", errors);
                    break;
                case "GRADE_OUT":
                    gradeOut = ParseRequiredNumber(fields, lineNumber, line, "Grade out", errors);
                    break;
                case "LENGTH":
                    curveLength = ParseRequiredNumber(fields, lineNumber, line, "Curve length", errors);
                    break;
                case "CHAINAGES":
                    if (fields.Length != 1)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: CHAINAGES section header does not take values."));
                        break;
                    }

                    readingChainages = true;
                    break;
                case "CHAINAGE":
                    var chainage = ParseRequiredNumber(fields, lineNumber, line, "Design chainage", errors);
                    if (chainage.HasValue)
                    {
                        chainages.Add(chainage.Value);
                    }

                    break;
                default:
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Unknown vertical curve keyword '{fields[0]}'."));
                    break;
            }
        }

        if (curveName is null)
        {
            errors.Add(new ParseError(0, string.Empty, "Vertical curve input requires VERTICAL_CURVE name."));
        }

        if (!pviChainage.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Vertical curve input requires PVI_CHAINAGE value."));
        }

        if (!pviElevation.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Vertical curve input requires PVI_ELEVATION value."));
        }

        if (!gradeIn.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Vertical curve input requires GRADE_IN value."));
        }

        if (!gradeOut.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Vertical curve input requires GRADE_OUT value."));
        }

        if (!curveLength.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Vertical curve input requires LENGTH value."));
        }

        if (chainages.Count == 0)
        {
            errors.Add(new ParseError(0, string.Empty, "Vertical curve input requires at least one design CHAINAGE."));
        }

        if (errors.Count > 0)
        {
            return new VerticalCurveParseResult(null, errors);
        }

        return new VerticalCurveParseResult(
            new VerticalCurveInput(
                curveName!,
                pviChainage!.Value,
                pviElevation!.Value,
                gradeIn!.Value,
                gradeOut!.Value,
                curveLength!.Value,
                chainages),
            errors);
    }

    public EarthworkParseResult ParseEarthwork(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var sections = new List<CrossSectionDefinition>();
        var errors = new List<ParseError>();
        double? currentChainage = null;
        double? currentDesignElevation = null;
        var currentPoints = new List<CrossSectionPoint>();

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
            if (keyword == "SECTION")
            {
                AddCurrentEarthworkSection(
                    sections,
                    currentChainage,
                    currentDesignElevation,
                    currentPoints);
                currentChainage = null;
                currentDesignElevation = null;
                currentPoints = new List<CrossSectionPoint>();

                if (fields.Length != 3)
                {
                    errors.Add(new ParseError(
                        lineNumber,
                        line,
                        $"Line {lineNumber}: SECTION requires chainage and design elevation."));
                    continue;
                }

                if (!TryParseNumber(fields[1], out var chainage) || !double.IsFinite(chainage))
                {
                    errors.Add(CreateNumericError(lineNumber, line, "Section chainage", fields[1]));
                    continue;
                }

                if (!TryParseNumber(fields[2], out var designElevation) || !double.IsFinite(designElevation))
                {
                    errors.Add(CreateNumericError(lineNumber, line, "Design elevation", fields[2]));
                    continue;
                }

                currentChainage = chainage;
                currentDesignElevation = designElevation;
                continue;
            }

            if (keyword == "END")
            {
                if (fields.Length != 1)
                {
                    errors.Add(new ParseError(
                        lineNumber,
                        line,
                        $"Line {lineNumber}: END does not take any values."));
                    continue;
                }

                if (!currentChainage.HasValue)
                {
                    errors.Add(new ParseError(
                        lineNumber,
                        line,
                        $"Line {lineNumber}: END appears without an active SECTION."));
                    continue;
                }

                AddCurrentEarthworkSection(
                    sections,
                    currentChainage,
                    currentDesignElevation,
                    currentPoints);
                currentChainage = null;
                currentDesignElevation = null;
                currentPoints = new List<CrossSectionPoint>();
                continue;
            }

            if (!currentChainage.HasValue)
            {
                errors.Add(new ParseError(
                    lineNumber,
                    line,
                    $"Line {lineNumber}: Cross-section point appears before a valid SECTION line."));
                continue;
            }

            if (fields.Length != 2)
            {
                errors.Add(new ParseError(
                    lineNumber,
                    line,
                    $"Line {lineNumber}: Cross-section point requires offset and ground elevation."));
                continue;
            }

            if (!TryParseNumber(fields[0], out var offset) || !double.IsFinite(offset))
            {
                errors.Add(CreateNumericError(lineNumber, line, "Offset", fields[0]));
                continue;
            }

            if (!TryParseNumber(fields[1], out var groundElevation) || !double.IsFinite(groundElevation))
            {
                errors.Add(CreateNumericError(lineNumber, line, "Ground elevation", fields[1]));
                continue;
            }

            currentPoints.Add(new CrossSectionPoint(offset, groundElevation));
        }

        AddCurrentEarthworkSection(
            sections,
            currentChainage,
            currentDesignElevation,
            currentPoints);

        if (sections.Count == 0)
        {
            errors.Add(new ParseError(
                0,
                string.Empty,
                "Earthwork input requires at least one SECTION chainage designElevation block."));
        }

        if (errors.Count > 0)
        {
            return new EarthworkParseResult(null, errors);
        }

        return new EarthworkParseResult(new EarthworkInput(sections), errors);
    }

    public ClothoidParseResult ParseClothoid(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string? curveName = null;
        double? startX = null;
        double? startY = null;
        double? azimuth = null;
        double? radius = null;
        double? length = null;
        string? direction = null;
        var distances = new List<double>();
        var errors = new List<ParseError>();
        var readingDistances = false;

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
            if (readingDistances && fields.Length == 1 && TryParseNumber(fields[0], out var distanceValue))
            {
                distances.Add(distanceValue);
                continue;
            }

            var keyword = fields[0].ToUpperInvariant();
            switch (keyword)
            {
                case "CLOTHOID":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: CLOTHOID requires a curve name."));
                    }
                    else
                    {
                        curveName = fields[1];
                    }

                    break;
                case "START":
                    if (fields.Length != 3 || !TryParseNumber(fields[1], out var parsedStartX) || !TryParseNumber(fields[2], out var parsedStartY))
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: START requires X and Y numeric values."));
                    }
                    else
                    {
                        startX = parsedStartX;
                        startY = parsedStartY;
                    }

                    break;
                case "AZIMUTH":
                    azimuth = ParseRequiredNumber(fields, lineNumber, line, "Azimuth", errors);
                    break;
                case "RADIUS":
                    radius = ParseRequiredNumber(fields, lineNumber, line, "Radius", errors);
                    break;
                case "LENGTH":
                    length = ParseRequiredNumber(fields, lineNumber, line, "Spiral length", errors);
                    break;
                case "DIRECTION":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: DIRECTION requires LEFT or RIGHT."));
                    }
                    else
                    {
                        direction = NormalizeDirectionText(fields[1]);
                    }

                    break;
                case "DISTANCES":
                    if (fields.Length != 1)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: DISTANCES does not take values."));
                    }
                    else
                    {
                        readingDistances = true;
                    }

                    break;
                case "DISTANCE":
                    var distance = ParseRequiredNumber(fields, lineNumber, line, "Distance", errors);
                    if (distance.HasValue)
                    {
                        distances.Add(distance.Value);
                    }

                    break;
                default:
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Unknown clothoid keyword '{fields[0]}'."));
                    break;
            }
        }

        if (curveName is null || !startX.HasValue || !startY.HasValue || !azimuth.HasValue || !radius.HasValue || !length.HasValue || direction is null)
        {
            errors.Add(new ParseError(0, string.Empty, "Clothoid input requires CLOTHOID, START, AZIMUTH, RADIUS, LENGTH, and DIRECTION."));
        }

        if (distances.Count == 0)
        {
            errors.Add(new ParseError(0, string.Empty, "Clothoid input requires at least one distance."));
        }

        return errors.Count > 0
            ? new ClothoidParseResult(null, errors)
            : new ClothoidParseResult(new ClothoidInput(curveName!, startX!.Value, startY!.Value, azimuth!.Value, radius!.Value, length!.Value, direction!, distances), errors);
    }

    public HorizontalAlignmentParseResult ParseHorizontalAlignment(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string? alignmentName = null;
        double? startChainage = null;
        double? startX = null;
        double? startY = null;
        double? azimuth = null;
        var elements = new List<AlignmentElementDefinition>();
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
                case "ALIGNMENT":
                    if (fields.Length != 2)
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: ALIGNMENT requires a name."));
                    }
                    else
                    {
                        alignmentName = fields[1];
                    }

                    break;
                case "START_CHAINAGE":
                    startChainage = ParseRequiredNumber(fields, lineNumber, line, "Start chainage", errors);
                    break;
                case "START":
                    if (fields.Length != 3 || !TryParseNumber(fields[1], out var parsedX) || !TryParseNumber(fields[2], out var parsedY))
                    {
                        errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: START requires X and Y numeric values."));
                    }
                    else
                    {
                        startX = parsedX;
                        startY = parsedY;
                    }

                    break;
                case "AZIMUTH":
                    azimuth = ParseRequiredNumber(fields, lineNumber, line, "Azimuth", errors);
                    break;
                case "ELEMENT":
                    ParseAlignmentElement(fields, lineNumber, line, elements, errors);
                    break;
                default:
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Unknown alignment keyword '{fields[0]}'."));
                    break;
            }
        }

        if (alignmentName is null || !startChainage.HasValue || !startX.HasValue || !startY.HasValue || !azimuth.HasValue)
        {
            errors.Add(new ParseError(0, string.Empty, "Alignment input requires ALIGNMENT, START_CHAINAGE, START, and AZIMUTH."));
        }

        if (elements.Count == 0)
        {
            errors.Add(new ParseError(0, string.Empty, "Alignment input requires at least one ELEMENT."));
        }

        return errors.Count > 0
            ? new HorizontalAlignmentParseResult(null, errors)
            : new HorizontalAlignmentParseResult(new HorizontalAlignmentInput(alignmentName!, startChainage!.Value, startX!.Value, startY!.Value, azimuth!.Value, elements), errors);
    }

    public ChainageListParseResult ParseChainages(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var chainages = new List<double>();
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

            if (!TryParseNumber(trimmed, out var chainage))
            {
                errors.Add(CreateNumericError(lineNumber, line, "Chainage", trimmed));
                continue;
            }

            chainages.Add(chainage);
        }

        if (chainages.Count == 0)
        {
            errors.Add(new ParseError(0, string.Empty, "At least one chainage is required."));
        }

        return new ChainageListParseResult(chainages, errors);
    }

    public CenterlineOffsetParseResult ParseCenterlineOffset(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var centerline = new List<CenterlinePoint>();
        var targets = new List<PointRecord>();
        var errors = new List<ParseError>();
        var section = string.Empty;

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
            if (fields.Length == 1 && string.Equals(fields[0], "CENTERLINE", StringComparison.OrdinalIgnoreCase))
            {
                section = "CENTERLINE";
                continue;
            }

            if (fields.Length == 1 && string.Equals(fields[0], "TARGETS", StringComparison.OrdinalIgnoreCase))
            {
                section = "TARGETS";
                continue;
            }

            if (section == "CENTERLINE")
            {
                if (fields.Length != 4 || !TryParseNumber(fields[1], out var chainage) || !TryParseNumber(fields[2], out var x) || !TryParseNumber(fields[3], out var y))
                {
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: CENTERLINE rows require Name Chainage X Y."));
                    continue;
                }

                centerline.Add(new CenterlinePoint(fields[0], chainage, x, y));
                continue;
            }

            if (section == "TARGETS")
            {
                if (fields.Length != 3 || !TryParseNumber(fields[1], out var x) || !TryParseNumber(fields[2], out var y))
                {
                    errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: TARGETS rows require Name X Y."));
                    continue;
                }

                targets.Add(new PointRecord(fields[0], x, y));
                continue;
            }

            errors.Add(new ParseError(lineNumber, line, $"Line {lineNumber}: Row must appear after CENTERLINE or TARGETS."));
        }

        if (centerline.Count == 0)
        {
            errors.Add(new ParseError(0, string.Empty, "Centerline offset input requires CENTERLINE rows."));
        }

        if (targets.Count == 0)
        {
            errors.Add(new ParseError(0, string.Empty, "Centerline offset input requires TARGETS rows."));
        }

        return errors.Count > 0
            ? new CenterlineOffsetParseResult(null, errors)
            : new CenterlineOffsetParseResult(new CenterlineOffsetInput(centerline, targets), errors);
    }

    private static void ParseAlignmentElement(
        IReadOnlyList<string> fields,
        int lineNumber,
        string rawLine,
        List<AlignmentElementDefinition> elements,
        List<ParseError> errors)
    {
        if (fields.Count < 3)
        {
            errors.Add(new ParseError(lineNumber, rawLine, $"Line {lineNumber}: ELEMENT requires type and name."));
            return;
        }

        var type = fields[1].ToUpperInvariant();
        var name = fields[2];
        double? length = null;
        double? radius = null;
        double? angle = null;
        string? direction = null;
        var reverse = false;
        for (var index = 3; index < fields.Count; index++)
        {
            var keyword = fields[index].ToUpperInvariant();
            if (keyword == "REVERSE")
            {
                reverse = true;
                continue;
            }

            if (index + 1 >= fields.Count)
            {
                errors.Add(new ParseError(lineNumber, rawLine, $"Line {lineNumber}: ELEMENT option {fields[index]} requires a value."));
                return;
            }

            var value = fields[++index];
            switch (keyword)
            {
                case "LENGTH":
                    if (!TryParseNumber(value, out var parsedLength))
                    {
                        errors.Add(CreateNumericError(lineNumber, rawLine, "Element length", value));
                    }
                    else
                    {
                        length = parsedLength;
                    }

                    break;
                case "RADIUS":
                    if (!TryParseNumber(value, out var parsedRadius))
                    {
                        errors.Add(CreateNumericError(lineNumber, rawLine, "Element radius", value));
                    }
                    else
                    {
                        radius = parsedRadius;
                    }

                    break;
                case "ANGLE":
                    if (!TryParseNumber(value, out var parsedAngle))
                    {
                        errors.Add(CreateNumericError(lineNumber, rawLine, "Arc angle", value));
                    }
                    else
                    {
                        angle = parsedAngle;
                    }

                    break;
                case "DIRECTION":
                    direction = NormalizeDirectionText(value);
                    break;
                default:
                    errors.Add(new ParseError(lineNumber, rawLine, $"Line {lineNumber}: Unknown ELEMENT option '{fields[index - 1]}'."));
                    break;
            }
        }

        if (type is not ("TANGENT" or "CLOTHOID" or "ARC"))
        {
            errors.Add(new ParseError(lineNumber, rawLine, $"Line {lineNumber}: Unsupported ELEMENT type '{fields[1]}'."));
            return;
        }

        elements.Add(new AlignmentElementDefinition(type, name, length, radius, angle, direction, reverse));
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

    private static void ParseTraverseQualityPoint(
        IReadOnlyList<string> fields,
        int lineNumber,
        string rawLine,
        List<PointRecord> points,
        List<ParseError> errors)
    {
        if (fields.Count is not (3 or 4))
        {
            errors.Add(new ParseError(lineNumber, rawLine, $"Line {lineNumber}: POINTS rows require Name X Y [H]."));
            return;
        }

        if (!TryParseNumber(fields[1], out var x))
        {
            errors.Add(CreateNumericError(lineNumber, rawLine, "X", fields[1]));
            return;
        }

        if (!TryParseNumber(fields[2], out var y))
        {
            errors.Add(CreateNumericError(lineNumber, rawLine, "Y", fields[2]));
            return;
        }

        double? h = null;
        if (fields.Count == 4)
        {
            if (!TryParseNumber(fields[3], out var parsedH))
            {
                errors.Add(CreateNumericError(lineNumber, rawLine, "H", fields[3]));
                return;
            }

            h = parsedH;
        }

        points.Add(new PointRecord(fields[0], x, y, h));
    }

    private static string NormalizeDirectionText(string direction)
    {
        if (string.Equals(direction, "Left", StringComparison.OrdinalIgnoreCase))
        {
            return "Left";
        }

        if (string.Equals(direction, "Right", StringComparison.OrdinalIgnoreCase))
        {
            return "Right";
        }

        return direction;
    }

    private static void AddCurrentEarthworkSection(
        List<CrossSectionDefinition> sections,
        double? chainage,
        double? designElevation,
        List<CrossSectionPoint> points)
    {
        if (chainage.HasValue && designElevation.HasValue)
        {
            sections.Add(new CrossSectionDefinition(
                chainage.Value,
                designElevation.Value,
                points));
        }
    }

    private static double? ParseRequiredNumber(
        IReadOnlyList<string> fields,
        int lineNumber,
        string rawLine,
        string fieldName,
        List<ParseError> errors)
    {
        if (fields.Count != 2)
        {
            errors.Add(new ParseError(lineNumber, rawLine, $"Line {lineNumber}: {fields[0]} requires one numeric value."));
            return null;
        }

        if (!TryParseNumber(fields[1], out var value))
        {
            errors.Add(CreateNumericError(lineNumber, rawLine, fieldName, fields[1]));
            return null;
        }

        return value;
    }
}
