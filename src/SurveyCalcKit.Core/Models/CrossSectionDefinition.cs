namespace SurveyCalcKit.Core.Models;

public sealed record CrossSectionDefinition(
    double Chainage,
    double DesignElevation,
    List<CrossSectionPoint> Points);
