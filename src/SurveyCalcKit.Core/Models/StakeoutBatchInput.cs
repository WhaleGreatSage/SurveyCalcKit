namespace SurveyCalcKit.Core.Models;

public sealed record StakeoutBatchInput(
    string OriginPointName,
    double OriginX,
    double OriginY,
    double BaselineAzimuthDegrees,
    double StartChainage,
    List<StakeoutRecord> Records);
