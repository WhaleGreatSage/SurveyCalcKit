namespace SurveyCalcKit.Core.Models;

public sealed record EarthworkIntervalResult(
    double FromChainage,
    double ToChainage,
    double Length,
    double CutVolume,
    double FillVolume,
    double NetVolume);
