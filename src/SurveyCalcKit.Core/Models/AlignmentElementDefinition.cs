namespace SurveyCalcKit.Core.Models;

public sealed record AlignmentElementDefinition(
    string Type,
    string Name,
    double? Length,
    double? Radius,
    double? AngleDegrees,
    string? Direction,
    bool Reverse);
