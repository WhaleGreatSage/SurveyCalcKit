namespace SurveyCalcKit.Core.Services;

internal static class AlignmentMath
{
    public const double CurvatureTolerance = 1e-10;

    public static double NormalizeAzimuth(double degrees)
    {
        if (!double.IsFinite(degrees))
        {
            return 0;
        }

        var normalized = degrees % 360.0;
        return normalized < 0 ? normalized + 360.0 : normalized;
    }

    public static bool TryGetDirectionSign(string? direction, out int sign)
    {
        if (string.Equals(direction, "LEFT", StringComparison.OrdinalIgnoreCase))
        {
            sign = 1;
            return true;
        }

        if (string.Equals(direction, "RIGHT", StringComparison.OrdinalIgnoreCase))
        {
            sign = -1;
            return true;
        }

        sign = 0;
        return false;
    }

    public static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    public static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

    public static double SimpsonIntegrate(Func<double, double> function, double upperBound, int intervals)
    {
        if (upperBound <= 0)
        {
            return 0;
        }

        var count = Math.Max(2, intervals);
        if (count % 2 != 0)
        {
            count++;
        }

        var step = upperBound / count;
        var sum = function(0) + function(upperBound);
        for (var index = 1; index < count; index++)
        {
            sum += (index % 2 == 0 ? 2 : 4) * function(index * step);
        }

        return sum * step / 3.0;
    }
}
