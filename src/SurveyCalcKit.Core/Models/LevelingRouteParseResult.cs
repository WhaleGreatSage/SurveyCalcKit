namespace SurveyCalcKit.Core.Models;

public sealed class LevelingRouteParseResult
{
    public LevelingRouteParseResult(LevelingRouteInput? route, IReadOnlyList<ParseError> errors)
    {
        Route = route;
        Errors = errors;
    }

    public LevelingRouteInput? Route { get; }

    public IReadOnlyList<ParseError> Errors { get; }

    public bool IsSuccess => Route is not null && Errors.Count == 0;
}
