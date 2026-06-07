namespace SurveyCalcKit.Core.Models;

public sealed record ChainageOffsetResult(
    string BaselineStartName,
    string BaselineEndName,
    string TargetPointName,
    double BaselineLength,
    double ProjectionRatio,
    double Chainage,
    double Offset,
    string Side,
    bool ProjectionInsideSegment,
    double ProjectionX,
    double ProjectionY,
    List<string> Warnings);
