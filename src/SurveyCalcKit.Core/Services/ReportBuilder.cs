using System.Globalization;
using System.Text;
using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class ReportBuilder
{
    private readonly TraverseCalculator traverseCalculator = new();

    public string BuildParseReport(ParseResult parseResult, ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Parse Report", "SurveyCalcKit 解析报告");
        AppendPointCount(builder, parseResult, language);
        AppendParseErrors(builder, parseResult, language);

        if (parseResult.Points.Count > 0)
        {
            AppendLine(builder, language, "Points:", "点表:");
            foreach (var point in parseResult.Points)
            {
                builder.AppendLine(FormatPoint(point));
            }
        }

        return builder.ToString();
    }

    public string BuildTraverseReport(
        ParseResult parseResult,
        IEnumerable<SegmentResult> segments,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(segments);

        var segmentList = segments.ToList();
        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Traverse Report", "SurveyCalcKit 导线计算报告");
        AppendPointCount(builder, parseResult, language);
        AppendSegmentTable(builder, segmentList, language);

        var total2D = traverseCalculator.CalculateTotal2DLength(segmentList);
        AppendLine(builder, language, $"Total 2D length: {FormatNumber(total2D)}", $"二维总长: {FormatNumber(total2D)}");
        AppendWarnings(builder, parseResult, segmentList, language);

        return builder.ToString();
    }

    public string BuildElevationReport(
        ParseResult parseResult,
        IEnumerable<SegmentResult> segments,
        double? elevationClosureError = null,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(segments);

        var segmentList = segments.ToList();
        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Elevation Report", "SurveyCalcKit 高程计算报告");
        AppendPointCount(builder, parseResult, language);
        AppendSegmentTable(builder, segmentList, language);

        if (elevationClosureError.HasValue)
        {
            AppendLine(
                builder,
                language,
                $"Elevation closure error: {FormatNumber(elevationClosureError.Value)}",
                $"高程闭合差: {FormatNumber(elevationClosureError.Value)}");
        }

        AppendWarnings(builder, parseResult, segmentList, language);

        return builder.ToString();
    }

    public string BuildTransformReport(
        ParseResult parseResult,
        IEnumerable<PointRecord> transformedPoints,
        double dx,
        double dy,
        double scale,
        double rotationAngleDegrees,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(transformedPoints);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Transform Report", "SurveyCalcKit 坐标变换报告");
        AppendPointCount(builder, parseResult, language);
        AppendLine(
            builder,
            language,
            $"Parameters: dx={FormatNumber(dx)}, dy={FormatNumber(dy)}, scale={FormatNumber(scale)}, angle={FormatNumber(rotationAngleDegrees)} deg",
            $"参数: dx={FormatNumber(dx)}, dy={FormatNumber(dy)}, scale={FormatNumber(scale)}, angle={FormatNumber(rotationAngleDegrees)} 度");
        AppendLine(builder, language, "Transformed points:", "变换后点表:");

        foreach (var point in transformedPoints)
        {
            builder.AppendLine(FormatPoint(point));
        }

        AppendParseErrors(builder, parseResult, language);
        return builder.ToString();
    }

    private static void AppendTitle(StringBuilder builder, ReportLanguage language, string english, string chinese)
    {
        AppendLine(builder, language, english, chinese);
        builder.AppendLine(new string('=', language == ReportLanguage.English ? english.Length : 24));
    }

    private static void AppendPointCount(StringBuilder builder, ParseResult parseResult, ReportLanguage language)
    {
        AppendLine(
            builder,
            language,
            $"Parsed point count: {parseResult.Points.Count}",
            $"解析点数: {parseResult.Points.Count}");
    }

    private static void AppendSegmentTable(StringBuilder builder, IReadOnlyList<SegmentResult> segments, ReportLanguage language)
    {
        if (segments.Count == 0)
        {
            AppendLine(builder, language, "Segments: none", "线段: 无");
            return;
        }

        AppendLine(builder, language, "Segments:", "线段表:");
        AppendLine(
            builder,
            language,
            "From -> To | Dx | Dy | Distance2D | Distance3D | Azimuth | DeltaH | Slope%",
            "起点 -> 终点 | Dx | Dy | 二维距离 | 三维距离 | 方位角 | 高差 | 坡度%");

        foreach (var segment in segments)
        {
            builder.AppendLine(
                $"{segment.From} -> {segment.To} | " +
                $"{FormatNumber(segment.Dx)} | {FormatNumber(segment.Dy)} | " +
                $"{FormatNumber(segment.Distance2D)} | {FormatNullable(segment.Distance3D)} | " +
                $"{FormatNumber(segment.AzimuthDegrees)} | {FormatNullable(segment.DeltaH)} | " +
                $"{FormatNullable(segment.SlopePercent)}");
        }
    }

    private static void AppendWarnings(
        StringBuilder builder,
        ParseResult parseResult,
        IReadOnlyList<SegmentResult> segments,
        ReportLanguage language)
    {
        var warnings = new List<string>();
        warnings.AddRange(parseResult.Errors.Select(error => error.Message));
        warnings.AddRange(segments
            .Where(segment => segment.Distance2D == 0)
            .Select(segment => language == ReportLanguage.English
                ? $"Segment {segment.From}->{segment.To} has zero horizontal distance; slope is not calculated."
                : $"线段 {segment.From}->{segment.To} 水平距离为 0，未计算坡度。"));
        warnings.AddRange(segments
            .Where(segment => !segment.DeltaH.HasValue)
            .Select(segment => language == ReportLanguage.English
                ? $"Segment {segment.From}->{segment.To} is missing elevation data."
                : $"线段 {segment.From}->{segment.To} 缺少高程数据。"));

        if (warnings.Count == 0)
        {
            AppendLine(builder, language, "Warnings: none", "警告: 无");
            return;
        }

        AppendLine(builder, language, "Warnings:", "警告:");
        foreach (var warning in warnings)
        {
            builder.AppendLine($"- {warning}");
        }
    }

    private static void AppendParseErrors(StringBuilder builder, ParseResult parseResult, ReportLanguage language)
    {
        if (parseResult.Errors.Count == 0)
        {
            AppendLine(builder, language, "Warnings: none", "警告: 无");
            return;
        }

        AppendLine(builder, language, "Warnings:", "警告:");
        foreach (var error in parseResult.Errors)
        {
            builder.AppendLine($"- {error.Message}");
        }
    }

    private static void AppendLine(StringBuilder builder, ReportLanguage language, string english, string chinese)
    {
        builder.AppendLine(language == ReportLanguage.English ? english : chinese);
    }

    private static string FormatPoint(PointRecord point)
    {
        return point.H.HasValue
            ? $"{point.Name}: X={FormatNumber(point.X)}, Y={FormatNumber(point.Y)}, H={FormatNumber(point.H.Value)}"
            : $"{point.Name}: X={FormatNumber(point.X)}, Y={FormatNumber(point.Y)}";
    }

    private static string FormatNullable(double? value)
    {
        return value.HasValue ? FormatNumber(value.Value) : "-";
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
