namespace SurveyCalcKit.Core.Models;

public sealed record AngleConversionResult(
    double DecimalDegrees,
    string DmsText,
    double Radians,
    List<string> Warnings);
