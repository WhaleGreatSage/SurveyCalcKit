namespace SurveyCalcKit.Core.Models;

public sealed record AlignmentState(
    double Chainage,
    double X,
    double Y,
    double AzimuthDegrees,
    double Curvature,
    string ElementType,
    string ElementName);
