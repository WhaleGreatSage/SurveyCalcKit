namespace SurveyCalcKit.Core.Models;

public sealed record CenterlineOffsetPointResult(
    string TargetPointName,
    double Chainage,
    double SignedOffset,
    double AbsoluteOffset,
    string Side,
    double ProjectionX,
    double ProjectionY,
    int SegmentIndex,
    string SegmentFrom,
    string SegmentTo,
    double DistanceToProjection,
    bool ProjectionInsideSelectedSegment);
