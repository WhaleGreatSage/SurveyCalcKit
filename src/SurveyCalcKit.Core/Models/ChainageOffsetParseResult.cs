namespace SurveyCalcKit.Core.Models;

public sealed class ChainageOffsetParseResult
{
    public ChainageOffsetParseResult(ChainageOffsetInput? input, IReadOnlyList<ParseError> errors)
    {
        Input = input;
        Errors = errors;
    }

    public ChainageOffsetInput? Input { get; }

    public IReadOnlyList<ParseError> Errors { get; }

    public bool IsSuccess => Input is not null && Errors.Count == 0;
}
