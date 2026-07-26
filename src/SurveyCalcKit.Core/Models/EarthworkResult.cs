namespace SurveyCalcKit.Core.Models;

public sealed record EarthworkResult(
    List<CrossSectionAreaResult> Sections,
    List<EarthworkIntervalResult> Intervals,
    double TotalCutVolume,
    double TotalFillVolume,
    double NetVolume,
    List<string> Warnings);
