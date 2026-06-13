namespace SurveyCalcKit.Core.Models;

public sealed record TraverseQualityInput(
    List<PointRecord> Points,
    List<double>? ObservedAnglesDegrees = null,
    double? AllowableRelativeClosureDenominator = null,
    double? AllowableAngularClosureSecondsPerStation = null,
    string? TraverseType = null);
