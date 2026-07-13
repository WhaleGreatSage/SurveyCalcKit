namespace SurveyCalcKit.Core.Models;

public sealed record GeoJsonImportResult(
    string GeometryType,
    List<PointRecord> Points,
    Dictionary<string, string> Metadata,
    List<string> Warnings);
