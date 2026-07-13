namespace SurveyCalcKit.Core.Models;

public sealed record HorizontalAlignmentResult(
    string AlignmentName,
    double StartChainage,
    double EndChainage,
    double TotalLength,
    List<AlignmentElementSummary> Elements,
    List<string> Warnings,
    HorizontalAlignment? Alignment);
