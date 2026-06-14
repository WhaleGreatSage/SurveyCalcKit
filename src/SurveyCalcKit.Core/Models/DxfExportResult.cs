namespace SurveyCalcKit.Core.Models;

public sealed record DxfExportResult(
    string OutputPath,
    int PointCount,
    bool PolylineExported,
    List<string> Warnings);
