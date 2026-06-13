namespace SurveyCalcKit.Core.Models;

public sealed record StakeoutBatchResult(
    string OriginPointName,
    double OriginX,
    double OriginY,
    double BaselineAzimuthDegrees,
    double StartChainage,
    List<StakeoutPointResult> Points,
    List<string> Warnings);
