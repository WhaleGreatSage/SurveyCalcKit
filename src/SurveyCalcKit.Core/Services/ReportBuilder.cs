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

    public string BuildClosureReport(
        ParseResult parseResult,
        TraverseClosureResult closureResult,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(closureResult);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Closed Traverse Report", "SurveyCalcKit 闭合导线平差报告");
        AppendPointCount(builder, parseResult, language);
        AppendLine(
            builder,
            language,
            $"Start: {closureResult.StartPointName} ({FormatNumber(closureResult.StartX)}, {FormatNumber(closureResult.StartY)})",
            $"起点: {closureResult.StartPointName} ({FormatNumber(closureResult.StartX)}, {FormatNumber(closureResult.StartY)})");
        AppendLine(
            builder,
            language,
            $"End: {closureResult.EndPointName} ({FormatNumber(closureResult.EndX)}, {FormatNumber(closureResult.EndY)})",
            $"终点: {closureResult.EndPointName} ({FormatNumber(closureResult.EndX)}, {FormatNumber(closureResult.EndY)})");
        AppendLine(
            builder,
            language,
            $"Closure fx={FormatNumber(closureResult.Fx)}, fy={FormatNumber(closureResult.Fy)}, f={FormatNumber(closureResult.ClosureError)}",
            $"闭合差 fx={FormatNumber(closureResult.Fx)}, fy={FormatNumber(closureResult.Fy)}, f={FormatNumber(closureResult.ClosureError)}");
        AppendLine(
            builder,
            language,
            $"Total length: {FormatNumber(closureResult.TotalLength)}",
            $"导线总长: {FormatNumber(closureResult.TotalLength)}");
        AppendLine(
            builder,
            language,
            $"Relative closure ratio: {FormatRatio(closureResult.RelativeClosureRatio)}",
            $"相对闭合差比例: {FormatRatio(closureResult.RelativeClosureRatio)}");

        AppendAdjustedSegmentTable(builder, closureResult.AdjustedSegments, language);
        AppendAdjustedPointTable(builder, closureResult.AdjustedPoints, language);
        AppendClosureWarnings(builder, closureResult.Warnings, language);

        return builder.ToString();
    }

    public string BuildLevelingParseReport(
        LevelingRouteParseResult parseResult,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Leveling Parse Report", "SurveyCalcKit 水准路线解析报告");
        if (parseResult.Route is not null)
        {
            AppendLine(
                builder,
                language,
                $"Start benchmark: {parseResult.Route.StartBenchmarkName}, elevation={FormatNumber(parseResult.Route.StartElevation)}",
                $"起始水准点: {parseResult.Route.StartBenchmarkName}, 高程={FormatNumber(parseResult.Route.StartElevation)}");
            AppendLine(
                builder,
                language,
                $"End benchmark: {parseResult.Route.EndBenchmarkName}, elevation={FormatNumber(parseResult.Route.EndElevation)}",
                $"终止水准点: {parseResult.Route.EndBenchmarkName}, 高程={FormatNumber(parseResult.Route.EndElevation)}");
            AppendLine(
                builder,
                language,
                $"Observation count: {parseResult.Route.Observations.Count}",
                $"观测站数: {parseResult.Route.Observations.Count}");
        }

        AppendParseErrors(builder, parseResult.Errors, language);
        return builder.ToString();
    }

    public string BuildLevelingRouteReport(
        LevelingRouteParseResult parseResult,
        LevelingRouteResult routeResult,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(routeResult);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Leveling Route Report", "SurveyCalcKit 水准路线闭合差与高程改正报告");
        AppendLine(
            builder,
            language,
            $"Start benchmark: {routeResult.StartBenchmarkName}, elevation={FormatNumber(routeResult.StartElevation)}",
            $"起始水准点: {routeResult.StartBenchmarkName}, 高程={FormatNumber(routeResult.StartElevation)}");
        AppendLine(
            builder,
            language,
            $"End benchmark: {routeResult.EndBenchmarkName}, elevation={FormatNumber(routeResult.EndElevation)}",
            $"终止水准点: {routeResult.EndBenchmarkName}, 高程={FormatNumber(routeResult.EndElevation)}");
        AppendLine(
            builder,
            language,
            $"Sum backsight: {FormatNumber(routeResult.SumBacksight)}",
            $"后视读数和: {FormatNumber(routeResult.SumBacksight)}");
        AppendLine(
            builder,
            language,
            $"Sum foresight: {FormatNumber(routeResult.SumForesight)}",
            $"前视读数和: {FormatNumber(routeResult.SumForesight)}");
        AppendLine(
            builder,
            language,
            $"Observed height difference: {FormatNumber(routeResult.ObservedHeightDifference)}",
            $"观测高差: {FormatNumber(routeResult.ObservedHeightDifference)}");
        AppendLine(
            builder,
            language,
            $"Known height difference: {FormatNumber(routeResult.KnownHeightDifference)}",
            $"已知高差: {FormatNumber(routeResult.KnownHeightDifference)}");
        AppendLine(
            builder,
            language,
            $"Closure error: {FormatNumber(routeResult.ClosureError)}",
            $"闭合差: {FormatNumber(routeResult.ClosureError)}");
        AppendLine(
            builder,
            language,
            $"Station count: {routeResult.StationCount}",
            $"测站数: {routeResult.StationCount}");
        AppendLine(
            builder,
            language,
            $"Correction per station: {FormatNumber(routeResult.CorrectionPerStation)}",
            $"每站改正数: {FormatNumber(routeResult.CorrectionPerStation)}");

        AppendLevelingPointTable(builder, routeResult.Points, language);
        AppendWarnings(builder, routeResult.Warnings, language);
        AppendParseErrors(builder, parseResult.Errors, language);
        return builder.ToString();
    }

    public string BuildCoordinateForwardReport(
        CoordinateForwardParseResult parseResult,
        CoordinateForwardResult result,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Coordinate Forward Report", "SurveyCalcKit 坐标正算报告");
        if (!parseResult.IsSuccess)
        {
            AppendParseErrors(builder, parseResult.Errors, language);
            return builder.ToString();
        }

        AppendLine(
            builder,
            language,
            $"Start point: {result.StartPointName}",
            $"起点: {result.StartPointName}");
        AppendLine(
            builder,
            language,
            $"Start coordinates: X={FormatNumber(result.StartX)}, Y={FormatNumber(result.StartY)}",
            $"起点坐标: X={FormatNumber(result.StartX)}, Y={FormatNumber(result.StartY)}");
        AppendLine(
            builder,
            language,
            $"Azimuth: {FormatNumber(result.AzimuthDegrees)} degrees",
            $"方位角: {FormatNumber(result.AzimuthDegrees)} 度");
        AppendLine(
            builder,
            language,
            $"Distance: {FormatNumber(result.Distance)}",
            $"距离: {FormatNumber(result.Distance)}");
        AppendLine(
            builder,
            language,
            $"Delta X: {FormatNumber(result.DeltaX)}",
            $"坐标增量 X: {FormatNumber(result.DeltaX)}");
        AppendLine(
            builder,
            language,
            $"Delta Y: {FormatNumber(result.DeltaY)}",
            $"坐标增量 Y: {FormatNumber(result.DeltaY)}");
        AppendLine(
            builder,
            language,
            $"End point: {result.EndPointName}",
            $"终点: {result.EndPointName}");
        AppendLine(
            builder,
            language,
            $"End coordinates: X={FormatNumber(result.EndX)}, Y={FormatNumber(result.EndY)}",
            $"终点坐标: X={FormatNumber(result.EndX)}, Y={FormatNumber(result.EndY)}");

        AppendWarnings(builder, result.Warnings, language);
        AppendParseErrors(builder, parseResult.Errors, language);
        return builder.ToString();
    }

    public string BuildChainageOffsetReport(
        ChainageOffsetParseResult parseResult,
        ChainageOffsetResult result,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Chainage/Offset Report", "SurveyCalcKit 里程与偏距计算报告");
        if (!parseResult.IsSuccess)
        {
            AppendParseErrors(builder, parseResult.Errors, language);
            return builder.ToString();
        }

        AppendLine(
            builder,
            language,
            $"Baseline: {result.BaselineStartName} -> {result.BaselineEndName}",
            $"基线: {result.BaselineStartName} -> {result.BaselineEndName}");
        AppendLine(
            builder,
            language,
            $"Target point: {result.TargetPointName}",
            $"目标点: {result.TargetPointName}");
        AppendLine(
            builder,
            language,
            $"Baseline length: {FormatNumber(result.BaselineLength)}",
            $"基线长度: {FormatNumber(result.BaselineLength)}");
        AppendLine(
            builder,
            language,
            $"Projection ratio: {FormatNumber(result.ProjectionRatio)}",
            $"投影比例: {FormatNumber(result.ProjectionRatio)}");
        AppendLine(
            builder,
            language,
            $"Projection coordinates: X={FormatNumber(result.ProjectionX)}, Y={FormatNumber(result.ProjectionY)}",
            $"投影点坐标: X={FormatNumber(result.ProjectionX)}, Y={FormatNumber(result.ProjectionY)}");
        AppendLine(
            builder,
            language,
            $"Chainage: {FormatNumber(result.Chainage)}",
            $"里程: {FormatNumber(result.Chainage)}");
        AppendLine(
            builder,
            language,
            $"Offset: {FormatNumber(result.Offset)}",
            $"偏距: {FormatNumber(result.Offset)}");
        AppendLine(
            builder,
            language,
            $"Side: {result.Side}",
            $"方向: {FormatSide(result.Side, language)}");
        AppendLine(
            builder,
            language,
            $"Projection inside segment: {FormatBoolean(result.ProjectionInsideSegment, language)}",
            $"投影位于线段内: {FormatBoolean(result.ProjectionInsideSegment, language)}");

        AppendWarnings(builder, result.Warnings, language);
        AppendParseErrors(builder, parseResult.Errors, language);
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

    private static void AppendAdjustedSegmentTable(
        StringBuilder builder,
        IReadOnlyList<AdjustedSegmentResult> segments,
        ReportLanguage language)
    {
        if (segments.Count == 0)
        {
            AppendLine(builder, language, "Adjusted segments: none", "改正后线段: 无");
            return;
        }

        AppendLine(builder, language, "Bowditch adjusted segments:", "Bowditch 改正线段表:");
        AppendLine(
            builder,
            language,
            "From -> To | Distance2D | OriginalDx | OriginalDy | CorrDx | CorrDy | AdjustedDx | AdjustedDy",
            "起点 -> 终点 | 二维距离 | 原Dx | 原Dy | 改正Dx | 改正Dy | 改正后Dx | 改正后Dy");

        foreach (var segment in segments)
        {
            builder.AppendLine(
                $"{segment.From} -> {segment.To} | " +
                $"{FormatNumber(segment.Distance2D)} | {FormatNumber(segment.OriginalDx)} | {FormatNumber(segment.OriginalDy)} | " +
                $"{FormatNumber(segment.CorrectionDx)} | {FormatNumber(segment.CorrectionDy)} | " +
                $"{FormatNumber(segment.AdjustedDx)} | {FormatNumber(segment.AdjustedDy)}");
        }
    }

    private static void AppendAdjustedPointTable(
        StringBuilder builder,
        IReadOnlyList<AdjustedPointRecord> points,
        ReportLanguage language)
    {
        if (points.Count == 0)
        {
            AppendLine(builder, language, "Adjusted points: none", "改正后坐标: 无");
            return;
        }

        AppendLine(builder, language, "Adjusted coordinates:", "改正后坐标表:");
        AppendLine(
            builder,
            language,
            "Name | OriginalX | OriginalY | CorrX | CorrY | AdjustedX | AdjustedY | H",
            "点名 | 原X | 原Y | 改正X | 改正Y | 改正后X | 改正后Y | H");

        foreach (var point in points)
        {
            builder.AppendLine(
                $"{point.Name} | {FormatNumber(point.OriginalX)} | {FormatNumber(point.OriginalY)} | " +
                $"{FormatNumber(point.CorrectionX)} | {FormatNumber(point.CorrectionY)} | " +
                $"{FormatNumber(point.AdjustedX)} | {FormatNumber(point.AdjustedY)} | {FormatNullable(point.H)}");
        }
    }

    private static void AppendClosureWarnings(
        StringBuilder builder,
        IReadOnlyList<string> warnings,
        ReportLanguage language)
    {
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

    private static void AppendLevelingPointTable(
        StringBuilder builder,
        IReadOnlyList<LevelingPointResult> points,
        ReportLanguage language)
    {
        if (points.Count == 0)
        {
            AppendLine(builder, language, "Adjusted elevations: none", "改正后高程: 无");
            return;
        }

        AppendLine(builder, language, "Adjusted elevations:", "改正后高程表:");
        AppendLine(
            builder,
            language,
            "Point | RawElevation | Correction | AdjustedElevation",
            "点名 | 未改正高程 | 改正数 | 改正后高程");

        foreach (var point in points)
        {
            builder.AppendLine(
                $"{point.PointName} | {FormatNumber(point.RawElevation)} | " +
                $"{FormatNumber(point.Correction)} | {FormatNumber(point.AdjustedElevation)}");
        }
    }

    private static void AppendWarnings(
        StringBuilder builder,
        IReadOnlyList<string> warnings,
        ReportLanguage language)
    {
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

    private static void AppendParseErrors(
        StringBuilder builder,
        IReadOnlyList<ParseError> errors,
        ReportLanguage language)
    {
        if (errors.Count == 0)
        {
            return;
        }

        AppendLine(builder, language, "Parse errors:", "解析错误:");
        foreach (var error in errors)
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
        if (Math.Abs(value) < 0.0005)
        {
            return "0";
        }

        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatRatio(double ratio)
    {
        return double.IsPositiveInfinity(ratio)
            ? "infinity (perfect closure)"
            : $"1:{FormatNumber(ratio)}";
    }

    private static string FormatBoolean(bool value, ReportLanguage language)
    {
        if (language == ReportLanguage.English)
        {
            return value ? "Yes" : "No";
        }

        return value ? "是" : "否";
    }

    private static string FormatSide(string side, ReportLanguage language)
    {
        if (language == ReportLanguage.English)
        {
            return side;
        }

        return side switch
        {
            "Left" => "左侧",
            "Right" => "右侧",
            "OnLine" => "在线上",
            "Undefined" => "未定义",
            _ => side
        };
    }
}
