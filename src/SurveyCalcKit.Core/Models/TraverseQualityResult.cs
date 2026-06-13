namespace SurveyCalcKit.Core.Models;

public sealed record TraverseQualityResult(
    int PointCount,
    int SegmentCount,
    double TotalLength,
    double Fx,
    double Fy,
    double LinearClosureError,
    double RelativeClosureDenominator,
    bool? PassesLinearClosureLimit,
    double? AngularClosureErrorSeconds,
    double? AllowableAngularClosureSeconds,
    bool? PassesAngularClosureLimit,
    string QualityGrade,
    List<string> Warnings,
    List<TraverseQualitySegmentRow> Segments);
