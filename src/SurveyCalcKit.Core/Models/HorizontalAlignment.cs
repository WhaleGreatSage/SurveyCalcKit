namespace SurveyCalcKit.Core.Models;

public sealed record HorizontalAlignment(
    string AlignmentName,
    double StartChainage,
    IReadOnlyList<IAlignmentElement> Elements);
