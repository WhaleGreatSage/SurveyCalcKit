namespace SurveyCalcKit.Core.Models;

public sealed record DxfExportOptions(
    string LayerName,
    bool ExportPoints,
    bool ExportPointLabels,
    bool ExportPolyline,
    bool ClosePolyline,
    double TextHeight);
