namespace SurveyCalcKit.Core.Models;

public sealed class CoordinateInverseParseResult
{
    public CoordinateInverseParseResult(CoordinateInverseInput? input, IReadOnlyList<ParseError> errors)
    {
        Input = input;
        Errors = errors;
    }

    public CoordinateInverseInput? Input { get; }

    public IReadOnlyList<ParseError> Errors { get; }

    public bool IsSuccess => Input is not null && Errors.Count == 0;
}
