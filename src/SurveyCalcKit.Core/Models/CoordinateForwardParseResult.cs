namespace SurveyCalcKit.Core.Models;

public sealed class CoordinateForwardParseResult
{
    public CoordinateForwardParseResult(CoordinateForwardInput? input, IReadOnlyList<ParseError> errors)
    {
        Input = input;
        Errors = errors;
    }

    public CoordinateForwardInput? Input { get; }

    public IReadOnlyList<ParseError> Errors { get; }

    public bool IsSuccess => Input is not null && Errors.Count == 0;
}
