using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class VerticalCurveCalculator
{
    private const double GradeDifferenceTolerance = 1e-9;

    public VerticalCurveResult Calculate(VerticalCurveInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var algebraicGradeDifferencePercent = input.GradeOutPercent - input.GradeInPercent;
        var curveType = DetermineCurveType(algebraicGradeDifferencePercent);

        if (!double.IsFinite(input.CurveLength) || input.CurveLength <= 0)
        {
            warnings.Add("Vertical curve length must be greater than zero.");
            return new VerticalCurveResult(
                input.CurveName,
                input.PviChainage,
                input.PviElevation,
                input.GradeInPercent,
                input.GradeOutPercent,
                algebraicGradeDifferencePercent,
                input.CurveLength,
                curveType,
                input.PviChainage,
                input.PviChainage,
                input.PviElevation,
                input.PviElevation,
                new List<VerticalCurvePointResult>(),
                warnings);
        }

        if (Math.Abs(algebraicGradeDifferencePercent) <= 0.0001)
        {
            warnings.Add("Algebraic grade difference is near zero; there is effectively no vertical curve.");
        }

        var g1 = input.GradeInPercent / 100.0;
        var g2 = input.GradeOutPercent / 100.0;
        var a = g2 - g1;
        var halfLength = input.CurveLength / 2.0;
        var pvcChainage = input.PviChainage - halfLength;
        var pvtChainage = input.PviChainage + halfLength;
        var pvcElevation = input.PviElevation - g1 * halfLength;
        var pvtElevation = input.PviElevation + g2 * halfLength;
        var points = new List<VerticalCurvePointResult>();

        foreach (var chainage in input.DesignChainages)
        {
            var distanceFromPvc = chainage - pvcChainage;
            var tangentElevation = pvcElevation + g1 * distanceFromPvc;
            var curveElevation = tangentElevation + (a / (2.0 * input.CurveLength)) * distanceFromPvc * distanceFromPvc;
            var isInside = chainage >= pvcChainage && chainage <= pvtChainage;
            if (!isInside)
            {
                warnings.Add($"Design chainage {chainage:0.###} is outside the vertical curve limits.");
            }

            points.Add(new VerticalCurvePointResult(
                chainage,
                tangentElevation,
                curveElevation,
                curveElevation - tangentElevation,
                isInside));
        }

        return new VerticalCurveResult(
            input.CurveName,
            input.PviChainage,
            input.PviElevation,
            input.GradeInPercent,
            input.GradeOutPercent,
            algebraicGradeDifferencePercent,
            input.CurveLength,
            curveType,
            pvcChainage,
            pvtChainage,
            pvcElevation,
            pvtElevation,
            points,
            warnings);
    }

    private static string DetermineCurveType(double algebraicGradeDifferencePercent)
    {
        if (algebraicGradeDifferencePercent > GradeDifferenceTolerance)
        {
            return "Sag";
        }

        if (algebraicGradeDifferencePercent < -GradeDifferenceTolerance)
        {
            return "Crest";
        }

        return "No vertical curve";
    }
}
