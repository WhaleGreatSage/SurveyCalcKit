namespace SurveyCalcKit.Core.Models;

public sealed record GeoJsonExportOptions(
    string GeometryType,
    string FeatureName,
    bool IncludeElevation,
    Dictionary<string, string> Properties);
