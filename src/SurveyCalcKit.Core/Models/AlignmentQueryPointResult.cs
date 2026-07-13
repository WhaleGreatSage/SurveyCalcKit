namespace SurveyCalcKit.Core.Models;

public sealed record AlignmentQueryPointResult(
    double Chainage,
    double X,
    double Y,
    double AzimuthDegrees,
    double Curvature,
    double Radius,
    string ElementType,
    string ElementName,
    bool IsInsideAlignment);
