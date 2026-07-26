using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class EarthworkCalculator
{
    private const double Tolerance = 1e-9;

    public EarthworkResult Calculate(EarthworkInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        if (input.Sections.Count == 0)
        {
            warnings.Add("Earthwork input contains no cross-sections.");
            return EmptyResult(warnings);
        }

        var originalChainages = input.Sections.Select(section => section.Chainage).ToList();
        var sections = input.Sections
            .Where(section => ValidateSectionHeader(section, warnings))
            .OrderBy(section => section.Chainage)
            .ToList();

        if (!originalChainages.SequenceEqual(originalChainages.OrderBy(value => value)))
        {
            warnings.Add("Cross-sections were not in ascending chainage order and were sorted before volume calculation.");
        }

        var sectionResults = new List<CrossSectionAreaResult>();
        foreach (var section in sections)
        {
            sectionResults.Add(CalculateSectionArea(section, warnings));
        }

        var intervals = new List<EarthworkIntervalResult>();
        for (var index = 1; index < sectionResults.Count; index++)
        {
            var from = sectionResults[index - 1];
            var to = sectionResults[index];
            var length = to.Chainage - from.Chainage;

            if (length <= Tolerance)
            {
                warnings.Add(
                    $"Cross-sections at chainages {from.Chainage:0.###} and {to.Chainage:0.###} do not define a positive interval; volume was skipped.");
                continue;
            }

            var cutVolume = (from.CutArea + to.CutArea) * length / 2.0;
            var fillVolume = (from.FillArea + to.FillArea) * length / 2.0;
            intervals.Add(new EarthworkIntervalResult(
                from.Chainage,
                to.Chainage,
                length,
                cutVolume,
                fillVolume,
                cutVolume - fillVolume));
        }

        if (sectionResults.Count < 2)
        {
            warnings.Add("At least two valid cross-sections are required to calculate earthwork volume.");
        }

        var totalCutVolume = intervals.Sum(interval => interval.CutVolume);
        var totalFillVolume = intervals.Sum(interval => interval.FillVolume);
        return new EarthworkResult(
            sectionResults,
            intervals,
            totalCutVolume,
            totalFillVolume,
            totalCutVolume - totalFillVolume,
            warnings);
    }

    private static CrossSectionAreaResult CalculateSectionArea(
        CrossSectionDefinition section,
        List<string> warnings)
    {
        var validPoints = section.Points
            .Where(point => ValidatePoint(section.Chainage, point, warnings))
            .OrderBy(point => point.Offset)
            .ToList();

        if (!section.Points.Select(point => point.Offset)
                .SequenceEqual(section.Points.Select(point => point.Offset).OrderBy(value => value)))
        {
            warnings.Add($"Cross-section {section.Chainage:0.###} points were sorted by offset.");
        }

        var uniquePoints = new List<CrossSectionPoint>();
        foreach (var point in validPoints)
        {
            if (uniquePoints.Count > 0 && Math.Abs(point.Offset - uniquePoints[^1].Offset) <= Tolerance)
            {
                warnings.Add(
                    $"Cross-section {section.Chainage:0.###} contains duplicate offset {point.Offset:0.###}; the later point was ignored.");
                continue;
            }

            uniquePoints.Add(point);
        }

        if (uniquePoints.Count < 2)
        {
            warnings.Add($"Cross-section {section.Chainage:0.###} requires at least two unique valid points.");
            return new CrossSectionAreaResult(
                section.Chainage,
                section.DesignElevation,
                uniquePoints.Count == 0 ? 0 : uniquePoints[0].Offset,
                uniquePoints.Count == 0 ? 0 : uniquePoints[^1].Offset,
                0,
                0,
                0,
                uniquePoints.Count);
        }

        var cutArea = 0.0;
        var fillArea = 0.0;
        for (var index = 1; index < uniquePoints.Count; index++)
        {
            var first = uniquePoints[index - 1];
            var second = uniquePoints[index];
            var width = second.Offset - first.Offset;
            var firstDifference = first.GroundElevation - section.DesignElevation;
            var secondDifference = second.GroundElevation - section.DesignElevation;

            AccumulateSegmentAreas(
                width,
                firstDifference,
                secondDifference,
                ref cutArea,
                ref fillArea);
        }

        return new CrossSectionAreaResult(
            section.Chainage,
            section.DesignElevation,
            uniquePoints[0].Offset,
            uniquePoints[^1].Offset,
            cutArea,
            fillArea,
            cutArea - fillArea,
            uniquePoints.Count);
    }

    private static void AccumulateSegmentAreas(
        double width,
        double firstDifference,
        double secondDifference,
        ref double cutArea,
        ref double fillArea)
    {
        if (firstDifference >= 0 && secondDifference >= 0)
        {
            cutArea += (firstDifference + secondDifference) * width / 2.0;
            return;
        }

        if (firstDifference <= 0 && secondDifference <= 0)
        {
            fillArea += (-firstDifference - secondDifference) * width / 2.0;
            return;
        }

        var firstWidth = width * Math.Abs(firstDifference) /
                         (Math.Abs(firstDifference) + Math.Abs(secondDifference));
        var secondWidth = width - firstWidth;
        if (firstDifference > 0)
        {
            cutArea += firstDifference * firstWidth / 2.0;
            fillArea += -secondDifference * secondWidth / 2.0;
        }
        else
        {
            fillArea += -firstDifference * firstWidth / 2.0;
            cutArea += secondDifference * secondWidth / 2.0;
        }
    }

    private static bool ValidateSectionHeader(CrossSectionDefinition section, List<string> warnings)
    {
        if (!double.IsFinite(section.Chainage) || !double.IsFinite(section.DesignElevation))
        {
            warnings.Add("A cross-section with a non-finite chainage or design elevation was ignored.");
            return false;
        }

        return true;
    }

    private static bool ValidatePoint(double chainage, CrossSectionPoint point, List<string> warnings)
    {
        if (double.IsFinite(point.Offset) && double.IsFinite(point.GroundElevation))
        {
            return true;
        }

        warnings.Add($"Cross-section {chainage:0.###} contains a non-finite point that was ignored.");
        return false;
    }

    private static EarthworkResult EmptyResult(List<string> warnings)
    {
        return new EarthworkResult(
            new List<CrossSectionAreaResult>(),
            new List<EarthworkIntervalResult>(),
            0,
            0,
            0,
            warnings);
    }
}
