using System.Globalization;
using System.Text;
using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class DxfExporter
{
    public DxfExportResult Export(
        IEnumerable<PointRecord> points,
        string outputPath,
        DxfExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(options);

        var pointList = points.ToList();
        var warnings = new List<string>();
        var layerName = string.IsNullOrWhiteSpace(options.LayerName) ? "SurveyCalcKit" : options.LayerName.Trim();
        var textHeight = double.IsFinite(options.TextHeight) && options.TextHeight > 0 ? options.TextHeight : 2.5;

        if (options.ExportPoints && pointList.Count == 0)
        {
            warnings.Add("At least one point is required for point export.");
        }

        var duplicateNames = pointList
            .Where(point => !string.IsNullOrWhiteSpace(point.Name))
            .GroupBy(point => point.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
        foreach (var name in duplicateNames)
        {
            warnings.Add($"Duplicate point label: {name}.");
        }

        if (pointList.Any(point => string.IsNullOrWhiteSpace(point.Name)))
        {
            warnings.Add("One or more point labels are empty.");
        }

        var polylineExported = options.ExportPolyline && pointList.Count >= 2;
        if (options.ExportPolyline && pointList.Count < 2)
        {
            warnings.Add("Polyline export requires at least two points.");
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var builder = new StringBuilder();
        AppendHeader(builder);
        if (options.ExportPoints)
        {
            foreach (var point in pointList)
            {
                AppendPoint(builder, layerName, point);
            }
        }

        if (options.ExportPointLabels)
        {
            foreach (var point in pointList.Where(point => !string.IsNullOrWhiteSpace(point.Name)))
            {
                AppendText(builder, layerName, point, textHeight);
            }
        }

        if (polylineExported)
        {
            AppendPolyline(builder, layerName, pointList, options.ClosePolyline);
        }

        AppendFooter(builder);
        File.WriteAllText(outputPath, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        return new DxfExportResult(outputPath, pointList.Count, polylineExported, warnings);
    }

    private static void AppendHeader(StringBuilder builder)
    {
        builder.AppendLine("0");
        builder.AppendLine("SECTION");
        builder.AppendLine("2");
        builder.AppendLine("ENTITIES");
    }

    private static void AppendFooter(StringBuilder builder)
    {
        builder.AppendLine("0");
        builder.AppendLine("ENDSEC");
        builder.AppendLine("0");
        builder.AppendLine("EOF");
    }

    private static void AppendPoint(StringBuilder builder, string layerName, PointRecord point)
    {
        builder.AppendLine("0");
        builder.AppendLine("POINT");
        builder.AppendLine("8");
        builder.AppendLine(layerName);
        builder.AppendLine("10");
        builder.AppendLine(FormatNumber(point.X));
        builder.AppendLine("20");
        builder.AppendLine(FormatNumber(point.Y));
        builder.AppendLine("30");
        builder.AppendLine(FormatNumber(point.H ?? 0));
    }

    private static void AppendText(StringBuilder builder, string layerName, PointRecord point, double textHeight)
    {
        builder.AppendLine("0");
        builder.AppendLine("TEXT");
        builder.AppendLine("8");
        builder.AppendLine(layerName);
        builder.AppendLine("10");
        builder.AppendLine(FormatNumber(point.X));
        builder.AppendLine("20");
        builder.AppendLine(FormatNumber(point.Y));
        builder.AppendLine("30");
        builder.AppendLine(FormatNumber(point.H ?? 0));
        builder.AppendLine("40");
        builder.AppendLine(FormatNumber(textHeight));
        builder.AppendLine("1");
        builder.AppendLine(point.Name);
    }

    private static void AppendPolyline(StringBuilder builder, string layerName, IReadOnlyList<PointRecord> points, bool closePolyline)
    {
        builder.AppendLine("0");
        builder.AppendLine("LWPOLYLINE");
        builder.AppendLine("8");
        builder.AppendLine(layerName);
        builder.AppendLine("90");
        builder.AppendLine(points.Count.ToString(CultureInfo.InvariantCulture));
        builder.AppendLine("70");
        builder.AppendLine(closePolyline ? "1" : "0");

        foreach (var point in points)
        {
            builder.AppendLine("10");
            builder.AppendLine(FormatNumber(point.X));
            builder.AppendLine("20");
            builder.AppendLine(FormatNumber(point.Y));
        }
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
