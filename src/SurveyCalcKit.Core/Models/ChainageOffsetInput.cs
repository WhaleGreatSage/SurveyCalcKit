namespace SurveyCalcKit.Core.Models;

public sealed record ChainageOffsetInput(
    string BaselineStartName,
    double StartX,
    double StartY,
    string BaselineEndName,
    double EndX,
    double EndY,
    string TargetPointName,
    double TargetX,
    double TargetY,
    double StartChainage = 0);
