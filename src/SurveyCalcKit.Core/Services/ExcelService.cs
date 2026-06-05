using ClosedXML.Excel;
using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class ExcelService
{
    public ExcelPointImportResult ImportPoints(string filePath)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var points = new List<PointRecord>();

        if (!ValidateReadableFile(filePath, errors))
        {
            return new ExcelPointImportResult(points, errors, warnings);
        }

        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet is null || worksheet.LastRowUsed() is null)
            {
                errors.Add("Excel workbook does not contain any point rows.");
                return new ExcelPointImportResult(points, errors, warnings);
            }

            var headerMap = BuildHeaderMap(worksheet);
            if (!headerMap.ContainsKey("Name") || !headerMap.ContainsKey("X") || !headerMap.ContainsKey("Y"))
            {
                errors.Add("Excel point import requires header columns: Name, X, Y. Optional column: H.");
                return new ExcelPointImportResult(points, errors, warnings);
            }

            var lastRowNumber = worksheet.LastRowUsed()!.RowNumber();
            for (var rowNumber = 2; rowNumber <= lastRowNumber; rowNumber++)
            {
                var name = worksheet.Cell(rowNumber, headerMap["Name"]).GetString().Trim();
                var xCell = worksheet.Cell(rowNumber, headerMap["X"]);
                var yCell = worksheet.Cell(rowNumber, headerMap["Y"]);
                var hCell = headerMap.TryGetValue("H", out var hColumn) ? worksheet.Cell(rowNumber, hColumn) : null;

                if (string.IsNullOrWhiteSpace(name) && xCell.IsEmpty() && yCell.IsEmpty() && (hCell is null || hCell.IsEmpty()))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add($"Row {rowNumber}: Name is required.");
                    continue;
                }

                if (!TryReadDouble(xCell, out var x))
                {
                    errors.Add($"Row {rowNumber}: X must be a numeric value.");
                    continue;
                }

                if (!TryReadDouble(yCell, out var y))
                {
                    errors.Add($"Row {rowNumber}: Y must be a numeric value.");
                    continue;
                }

                double? h = null;
                if (hCell is not null && !hCell.IsEmpty())
                {
                    if (!TryReadDouble(hCell, out var parsedH))
                    {
                        errors.Add($"Row {rowNumber}: H must be a numeric value when provided.");
                        continue;
                    }

                    h = parsedH;
                }

                points.Add(new PointRecord(name, x, y, h));
            }

            if (points.Count == 0 && errors.Count == 0)
            {
                errors.Add("Excel workbook does not contain any valid point records.");
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or FormatException)
        {
            errors.Add($"Could not read Excel workbook: {ex.Message}");
        }

        return new ExcelPointImportResult(points, errors, warnings);
    }

    public ExcelExportResult ExportPoints(string filePath, IEnumerable<PointRecord> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        return ExportWorkbook(filePath, workbook =>
        {
            var worksheet = workbook.AddWorksheet("Points");
            WriteHeaders(worksheet, "Name", "X", "Y", "H");

            var row = 2;
            foreach (var point in points)
            {
                worksheet.Cell(row, 1).Value = point.Name;
                worksheet.Cell(row, 2).Value = point.X;
                worksheet.Cell(row, 3).Value = point.Y;
                if (point.H.HasValue)
                {
                    worksheet.Cell(row, 4).Value = point.H.Value;
                }

                row++;
            }
        });
    }

    public ExcelExportResult ExportTraverseResults(string filePath, IEnumerable<SegmentResult> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        return ExportWorkbook(filePath, workbook =>
        {
            var worksheet = workbook.AddWorksheet("Traverse");
            WriteHeaders(
                worksheet,
                "From",
                "To",
                "Dx",
                "Dy",
                "Distance2D",
                "Distance3D",
                "AzimuthDegrees",
                "DeltaH",
                "SlopePercent");

            var row = 2;
            foreach (var segment in segments)
            {
                worksheet.Cell(row, 1).Value = segment.From;
                worksheet.Cell(row, 2).Value = segment.To;
                worksheet.Cell(row, 3).Value = segment.Dx;
                worksheet.Cell(row, 4).Value = segment.Dy;
                worksheet.Cell(row, 5).Value = segment.Distance2D;
                if (segment.Distance3D.HasValue)
                {
                    worksheet.Cell(row, 6).Value = segment.Distance3D.Value;
                }

                worksheet.Cell(row, 7).Value = segment.AzimuthDegrees;
                if (segment.DeltaH.HasValue)
                {
                    worksheet.Cell(row, 8).Value = segment.DeltaH.Value;
                }

                if (segment.SlopePercent.HasValue)
                {
                    worksheet.Cell(row, 9).Value = segment.SlopePercent.Value;
                }

                row++;
            }
        });
    }

    public ExcelExportResult ExportLevelingResults(string filePath, LevelingRouteResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return ExportWorkbook(filePath, workbook =>
        {
            var summary = workbook.AddWorksheet("Leveling Summary");
            WriteHeaders(summary, "Field", "Value");
            WriteSummaryRow(summary, 2, "StartBenchmarkName", result.StartBenchmarkName);
            WriteSummaryRow(summary, 3, "StartElevation", result.StartElevation);
            WriteSummaryRow(summary, 4, "EndBenchmarkName", result.EndBenchmarkName);
            WriteSummaryRow(summary, 5, "EndElevation", result.EndElevation);
            WriteSummaryRow(summary, 6, "SumBacksight", result.SumBacksight);
            WriteSummaryRow(summary, 7, "SumForesight", result.SumForesight);
            WriteSummaryRow(summary, 8, "ObservedHeightDifference", result.ObservedHeightDifference);
            WriteSummaryRow(summary, 9, "KnownHeightDifference", result.KnownHeightDifference);
            WriteSummaryRow(summary, 10, "ClosureError", result.ClosureError);
            WriteSummaryRow(summary, 11, "StationCount", result.StationCount);
            WriteSummaryRow(summary, 12, "CorrectionPerStation", result.CorrectionPerStation);

            var points = workbook.AddWorksheet("Adjusted Elevations");
            WriteHeaders(points, "PointName", "RawElevation", "Correction", "AdjustedElevation");
            var row = 2;
            foreach (var point in result.Points)
            {
                points.Cell(row, 1).Value = point.PointName;
                points.Cell(row, 2).Value = point.RawElevation;
                points.Cell(row, 3).Value = point.Correction;
                points.Cell(row, 4).Value = point.AdjustedElevation;
                row++;
            }
        });
    }

    public ExcelExportResult ExportPolygonAreaResults(
        string filePath,
        IEnumerable<PointRecord> points,
        double area,
        string areaUnit = "square units")
    {
        ArgumentNullException.ThrowIfNull(points);

        return ExportWorkbook(filePath, workbook =>
        {
            var summary = workbook.AddWorksheet("Polygon Area");
            WriteHeaders(summary, "Field", "Value");
            WriteSummaryRow(summary, 2, "Area", area);
            WriteSummaryRow(summary, 3, "AreaUnit", areaUnit);

            var pointSheet = workbook.AddWorksheet("Polygon Points");
            WriteHeaders(pointSheet, "Name", "X", "Y", "H");
            var row = 2;
            foreach (var point in points)
            {
                pointSheet.Cell(row, 1).Value = point.Name;
                pointSheet.Cell(row, 2).Value = point.X;
                pointSheet.Cell(row, 3).Value = point.Y;
                if (point.H.HasValue)
                {
                    pointSheet.Cell(row, 4).Value = point.H.Value;
                }

                row++;
            }
        });
    }

    public ExcelExportResult ExportReportText(string filePath, string title, string reportText)
    {
        return ExportWorkbook(filePath, workbook =>
        {
            var worksheet = workbook.AddWorksheet("Report");
            worksheet.Cell(1, 1).Value = title;
            worksheet.Cell(1, 1).Style.Font.Bold = true;

            var lines = (reportText ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                worksheet.Cell(i + 2, 1).Value = lines[i];
            }
        });
    }

    private static bool ValidateReadableFile(string filePath, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            errors.Add("Excel file path is required.");
            return false;
        }

        if (!File.Exists(filePath))
        {
            errors.Add($"Excel file was not found: {filePath}");
            return false;
        }

        if (new FileInfo(filePath).Length == 0)
        {
            errors.Add("Excel file is empty.");
            return false;
        }

        if (!string.Equals(Path.GetExtension(filePath), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Excel import currently supports .xlsx files.");
            return false;
        }

        return true;
    }

    private static ExcelExportResult ExportWorkbook(string filePath, Action<XLWorkbook> fillWorkbook)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            errors.Add("Excel export file path is required.");
            return new ExcelExportResult(filePath, errors, warnings);
        }

        if (!string.Equals(Path.GetExtension(filePath), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Excel export file path must end with .xlsx.");
            return new ExcelExportResult(filePath, errors, warnings);
        }

        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var workbook = new XLWorkbook();
            fillWorkbook(workbook);
            foreach (var worksheet in workbook.Worksheets)
            {
                worksheet.Columns().AdjustToContents();
            }

            workbook.SaveAs(filePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            errors.Add($"Could not write Excel workbook: {ex.Message}");
        }

        return new ExcelExportResult(filePath, errors, warnings);
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet worksheet)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastColumn = worksheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (var column = 1; column <= lastColumn; column++)
        {
            var header = worksheet.Cell(1, column).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(header) && !map.ContainsKey(header))
            {
                map.Add(header, column);
            }
        }

        return map;
    }

    private static bool TryReadDouble(IXLCell cell, out double value)
    {
        value = 0;
        if (cell.TryGetValue<double>(out var numericValue))
        {
            value = numericValue;
            return true;
        }

        return double.TryParse(cell.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static void WriteHeaders(IXLWorksheet worksheet, params string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }
    }

    private static void WriteSummaryRow(IXLWorksheet worksheet, int row, string field, object value)
    {
        worksheet.Cell(row, 1).Value = field;
        worksheet.Cell(row, 2).Value = XLCellValue.FromObject(value);
    }
}
