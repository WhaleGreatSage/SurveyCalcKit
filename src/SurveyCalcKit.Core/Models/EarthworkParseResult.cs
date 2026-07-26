namespace SurveyCalcKit.Core.Models;

public sealed record EarthworkParseResult(
    EarthworkInput? Input,
    IReadOnlyList<ParseError> Errors)
{
    public bool IsSuccess => Input is not null && Errors.Count == 0;
}
