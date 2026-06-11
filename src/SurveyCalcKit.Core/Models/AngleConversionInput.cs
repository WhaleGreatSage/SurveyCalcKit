namespace SurveyCalcKit.Core.Models;

public sealed record AngleConversionInput(
    double? DecimalDegrees,
    string? DmsText,
    double? Radians,
    bool NormalizeAzimuth = false);
