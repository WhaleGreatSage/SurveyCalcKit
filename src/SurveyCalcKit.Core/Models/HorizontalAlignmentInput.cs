namespace SurveyCalcKit.Core.Models;

public sealed record HorizontalAlignmentInput(
    string AlignmentName,
    double StartChainage,
    double StartX,
    double StartY,
    double StartAzimuthDegrees,
    List<AlignmentElementDefinition> Elements);
