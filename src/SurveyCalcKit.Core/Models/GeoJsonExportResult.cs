namespace SurveyCalcKit.Core.Models;

public sealed record GeoJsonExportResult(
    string OutputPath,
    string GeometryType,
    int CoordinateCount,
    List<string> Warnings);
