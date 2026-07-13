namespace SurveyCalcKit.Core.Models;

public sealed record AlignmentElementSummary(
    string ElementType,
    string ElementName,
    double StartChainage,
    double EndChainage,
    double Length,
    double StartX,
    double StartY,
    double EndX,
    double EndY,
    double StartAzimuthDegrees,
    double EndAzimuthDegrees,
    double StartCurvature,
    double EndCurvature);
