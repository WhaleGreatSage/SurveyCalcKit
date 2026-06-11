using System.Globalization;
using System.Text.RegularExpressions;
using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class AngleConverter
{
    public AngleConversionResult Convert(AngleConversionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        double decimalDegrees;

        if (input.DecimalDegrees.HasValue)
        {
            decimalDegrees = input.DecimalDegrees.Value;
        }
        else if (!string.IsNullOrWhiteSpace(input.DmsText))
        {
            if (!TryParseDms(input.DmsText, out decimalDegrees))
            {
                warnings.Add($"Invalid DMS angle text '{input.DmsText}'. Use examples such as 53 7 48.37 or 53:7:48.37.");
                decimalDegrees = 0;
            }
        }
        else if (input.Radians.HasValue)
        {
            decimalDegrees = input.Radians.Value * 180.0 / Math.PI;
        }
        else
        {
            warnings.Add("No angle value was provided.");
            decimalDegrees = 0;
        }

        if (!double.IsFinite(decimalDegrees))
        {
            warnings.Add("Angle is not a finite number; 0 degrees was used.");
            decimalDegrees = 0;
        }

        if (input.NormalizeAzimuth)
        {
            decimalDegrees = NormalizeDegrees(decimalDegrees);
        }

        return new AngleConversionResult(
            decimalDegrees,
            FormatDms(decimalDegrees),
            decimalDegrees * Math.PI / 180.0,
            warnings);
    }

    private static bool TryParseDms(string text, out double decimalDegrees)
    {
        decimalDegrees = 0;
        var matches = Regex.Matches(text, @"[-+]?\d+(?:\.\d+)?");
        if (matches.Count is < 1 or > 3)
        {
            return false;
        }

        var values = matches
            .Select(match => double.Parse(match.Value, CultureInfo.InvariantCulture))
            .ToArray();

        var sign = text.TrimStart().StartsWith("-", StringComparison.Ordinal) || values[0] < 0 ? -1 : 1;
        var degrees = Math.Abs(values[0]);
        var minutes = values.Length > 1 ? Math.Abs(values[1]) : 0;
        var seconds = values.Length > 2 ? Math.Abs(values[2]) : 0;

        if (minutes >= 60 || seconds >= 60)
        {
            return false;
        }

        decimalDegrees = sign * (degrees + minutes / 60.0 + seconds / 3600.0);
        return true;
    }

    private static string FormatDms(double decimalDegrees)
    {
        var sign = decimalDegrees < 0 ? "-" : string.Empty;
        var absolute = Math.Abs(decimalDegrees);
        var degrees = (int)Math.Floor(absolute);
        var minuteFloat = (absolute - degrees) * 60.0;
        var minutes = (int)Math.Floor(minuteFloat);
        var seconds = Math.Round((minuteFloat - minutes) * 60.0, 2, MidpointRounding.AwayFromZero);

        if (seconds >= 60)
        {
            seconds = 0;
            minutes++;
        }

        if (minutes >= 60)
        {
            minutes = 0;
            degrees++;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0}{1}°{2:00}'{3:00.00}\"", sign, degrees, minutes, seconds);
    }

    private static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360.0;
        return normalized < 0 ? normalized + 360.0 : normalized;
    }
}
