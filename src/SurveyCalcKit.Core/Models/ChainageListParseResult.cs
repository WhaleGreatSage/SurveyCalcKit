namespace SurveyCalcKit.Core.Models;

public sealed record ChainageListParseResult(List<double> Chainages, IReadOnlyList<ParseError> Errors)
{
    public bool IsSuccess => Errors.Count == 0;
}
