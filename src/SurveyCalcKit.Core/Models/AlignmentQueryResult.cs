namespace SurveyCalcKit.Core.Models;

public sealed record AlignmentQueryResult(
    string AlignmentName,
    List<AlignmentQueryPointResult> Points,
    List<string> Warnings);
