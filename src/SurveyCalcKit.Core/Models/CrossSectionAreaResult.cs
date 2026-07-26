namespace SurveyCalcKit.Core.Models;

public sealed record CrossSectionAreaResult(
    double Chainage,
    double DesignElevation,
    double MinimumOffset,
    double MaximumOffset,
    double CutArea,
    double FillArea,
    double NetArea,
    int PointCount);
