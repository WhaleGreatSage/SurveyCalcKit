using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class EarthworkCalculatorTests
{
    [Fact]
    public void ParseEarthwork_ParsesSpaceAndCommaSeparatedSections()
    {
        var result = new ParseService().ParseEarthwork(
            """
            SECTION 0.000 100.000
            -5.000 101.000
            5.000 101.000
            END

            SECTION,20.000,100.500
            -5.000,99.500
            5.000,99.500
            END
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Input!.Sections.Count);
        Assert.Equal(20, result.Input.Sections[1].Chainage, 6);
        Assert.Equal(100.5, result.Input.Sections[1].DesignElevation, 6);
        Assert.Equal(2, result.Input.Sections[1].Points.Count);
    }

    [Fact]
    public void ParseEarthwork_RejectsInvalidNumericField()
    {
        var result = new ParseService().ParseEarthwork(
            """
            SECTION 0.000 100.000
            left 101.000
            END
            """);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Message.Contains("Offset", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseEarthwork_RejectsPointBeforeSection()
    {
        var result = new ParseService().ParseEarthwork("-5.000 101.000");

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Message.Contains("before", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_ComputesPureCutArea()
    {
        var result = new EarthworkCalculator().Calculate(
            CreateInput(CreateSection(0, 100, (-5, 101), (5, 101))));

        Assert.Equal(10, result.Sections.Single().CutArea, 6);
        Assert.Equal(0, result.Sections.Single().FillArea, 6);
    }

    [Fact]
    public void Calculate_ComputesPureFillArea()
    {
        var result = new EarthworkCalculator().Calculate(
            CreateInput(CreateSection(0, 100, (-5, 99), (5, 99))));

        Assert.Equal(0, result.Sections.Single().CutArea, 6);
        Assert.Equal(10, result.Sections.Single().FillArea, 6);
    }

    [Fact]
    public void Calculate_SplitsAreaAtGroundDesignCrossing()
    {
        var result = new EarthworkCalculator().Calculate(
            CreateInput(CreateSection(0, 100, (-5, 101), (5, 99))));

        Assert.Equal(2.5, result.Sections.Single().CutArea, 6);
        Assert.Equal(2.5, result.Sections.Single().FillArea, 6);
        Assert.Equal(0, result.Sections.Single().NetArea, 6);
    }

    [Fact]
    public void Calculate_UsesAverageEndAreaMethod()
    {
        var result = new EarthworkCalculator().Calculate(
            CreateInput(
                CreateSection(0, 100, (-5, 101), (5, 101)),
                CreateSection(20, 100, (-5, 102), (5, 102))));

        var interval = Assert.Single(result.Intervals);
        Assert.Equal(20, interval.Length, 6);
        Assert.Equal(300, interval.CutVolume, 6);
        Assert.Equal(0, interval.FillVolume, 6);
        Assert.Equal(300, result.TotalCutVolume, 6);
    }

    [Fact]
    public void Calculate_TracksCutAndFillTotalsSeparately()
    {
        var result = new EarthworkCalculator().Calculate(
            CreateInput(
                CreateSection(0, 100, (-5, 101), (5, 101)),
                CreateSection(10, 100, (-5, 99), (5, 99))));

        var interval = Assert.Single(result.Intervals);
        Assert.Equal(50, interval.CutVolume, 6);
        Assert.Equal(50, interval.FillVolume, 6);
        Assert.Equal(0, interval.NetVolume, 6);
    }

    [Fact]
    public void Calculate_SortsChainagesAndOffsetsWithWarnings()
    {
        var result = new EarthworkCalculator().Calculate(
            CreateInput(
                CreateSection(20, 100, (5, 101), (-5, 101)),
                CreateSection(0, 100, (5, 101), (-5, 101))));

        Assert.Equal(0, result.Sections[0].Chainage, 6);
        Assert.Equal(-5, result.Sections[0].MinimumOffset, 6);
        Assert.Contains(result.Warnings, warning => warning.Contains("ascending chainage", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Contains("sorted by offset", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_ProtectsDuplicateOffsetsAndZeroLengthIntervals()
    {
        var result = new EarthworkCalculator().Calculate(
            CreateInput(
                CreateSection(0, 100, (-5, 101), (-5, 102), (5, 101)),
                CreateSection(0, 100, (-5, 101), (5, 101))));

        Assert.Empty(result.Intervals);
        Assert.Contains(result.Warnings, warning => warning.Contains("duplicate offset", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Contains("positive interval", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Calculate_WarnsWhenOnlyOneSectionIsAvailable()
    {
        var result = new EarthworkCalculator().Calculate(
            CreateInput(CreateSection(0, 100, (-5, 101), (5, 101))));

        Assert.Empty(result.Intervals);
        Assert.Contains(result.Warnings, warning => warning.Contains("at least two", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BuildEarthworkReport_GeneratesChineseAndEnglishReports()
    {
        var parser = new ParseService();
        var parseResult = parser.ParseEarthwork(
            """
            SECTION 0 100
            -5 101
            5 101
            SECTION 10 100
            -5 101
            5 101
            """);
        var calculation = new EarthworkCalculator().Calculate(parseResult.Input!);
        var builder = new ReportBuilder();

        var english = builder.BuildEarthworkReport(parseResult, calculation, ReportLanguage.English);
        var chinese = builder.BuildEarthworkReport(parseResult, calculation, ReportLanguage.Chinese);

        Assert.Contains("Total cut volume", english);
        Assert.Contains("总挖方体积", chinese);
        Assert.Contains("平均断面法体积表", chinese);
    }

    private static EarthworkInput CreateInput(params CrossSectionDefinition[] sections)
    {
        return new EarthworkInput(sections.ToList());
    }

    private static CrossSectionDefinition CreateSection(
        double chainage,
        double designElevation,
        params (double Offset, double Elevation)[] points)
    {
        return new CrossSectionDefinition(
            chainage,
            designElevation,
            points.Select(point => new CrossSectionPoint(point.Offset, point.Elevation)).ToList());
    }
}
