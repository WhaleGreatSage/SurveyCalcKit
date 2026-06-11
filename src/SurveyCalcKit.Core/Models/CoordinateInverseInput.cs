namespace SurveyCalcKit.Core.Models;

public sealed record CoordinateInverseInput(
    string FromPointName,
    double FromX,
    double FromY,
    double? FromH,
    string ToPointName,
    double ToX,
    double ToY,
    double? ToH);
