namespace SurveyCalcKit.Core.Models;

public sealed record TraverseQualityParseResult(
    TraverseQualityInput? Input,
    IReadOnlyList<ParseError> Errors)
{
    public bool IsSuccess => Input is not null && Errors.Count == 0;
}
