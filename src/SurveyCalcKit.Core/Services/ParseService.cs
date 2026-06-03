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
