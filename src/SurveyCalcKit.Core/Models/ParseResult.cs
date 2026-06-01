namespace SurveyCalcKit.Core.Models;

public sealed class ParseResult
{
    public ParseResult(IReadOnlyList<PointRecord> points, IReadOnlyList<ParseError> errors)
    {
        Points = points;
        Errors = errors;
    }

    public IReadOnlyList<PointRecord> Points { get; }

    public IReadOnlyList<ParseError> Errors { get; }

    public bool IsSuccess => Errors.Count == 0;
}
