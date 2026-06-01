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
