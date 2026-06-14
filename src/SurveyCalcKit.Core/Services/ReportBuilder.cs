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

    public string BuildCoordinateInverseReport(
        CoordinateInverseParseResult parseResult,
        CoordinateInverseResult result,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Coordinate Inverse Report", "SurveyCalcKit 坐标反算报告");
        if (!parseResult.IsSuccess)
        {
            AppendParseErrors(builder, parseResult.Errors, language);
            return builder.ToString();
        }

        AppendLine(builder, language, $"From point: {result.FromPointName}", $"起点: {result.FromPointName}");
        AppendLine(builder, language, $"To point: {result.ToPointName}", $"终点: {result.ToPointName}");
        AppendLine(builder, language, $"Delta X: {FormatNumber(result.DeltaX)}", $"坐标增量 X: {FormatNumber(result.DeltaX)}");
        AppendLine(builder, language, $"Delta Y: {FormatNumber(result.DeltaY)}", $"坐标增量 Y: {FormatNumber(result.DeltaY)}");
        AppendLine(builder, language, $"Distance 2D: {FormatNumber(result.Distance2D)}", $"二维距离: {FormatNumber(result.Distance2D)}");
        AppendLine(builder, language, $"Azimuth: {FormatNumber(result.AzimuthDegrees)} degrees", $"方位角: {FormatNumber(result.AzimuthDegrees)} 度");
        AppendLine(builder, language, $"Delta H: {FormatNullable(result.DeltaH)}", $"高差: {FormatNullable(result.DeltaH)}");
        AppendLine(builder, language, $"Distance 3D: {FormatNullable(result.Distance3D)}", $"三维距离: {FormatNullable(result.Distance3D)}");
        AppendWarnings(builder, result.Warnings, language);
        AppendParseErrors(builder, parseResult.Errors, language);
        return builder.ToString();
    }

    public string BuildBatchSegmentTableReport(
        ParseResult parseResult,
        BatchSegmentTableResult result,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Batch Segment Table Report", "SurveyCalcKit 批量边长方位角表");
        AppendPointCount(builder, parseResult, language);
        AppendLine(builder, language, $"Segment count: {result.SegmentCount}", $"线段数: {result.SegmentCount}");
        AppendLine(builder, language, $"Total length: {FormatNumber(result.TotalLength)}", $"总长度: {FormatNumber(result.TotalLength)}");
        AppendBatchSegmentRows(builder, result.Rows, language);
        AppendWarnings(builder, result.Warnings, language);
        AppendParseErrors(builder, parseResult.Errors, language);
        return builder.ToString();
    }

    public string BuildAngleConversionReport(
        AngleConversionResult result,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Angle Conversion Report", "SurveyCalcKit 角度格式转换报告");
        AppendLine(builder, language, $"Decimal degrees: {FormatNumber(result.DecimalDegrees)}", $"十进制度: {FormatNumber(result.DecimalDegrees)}");
        AppendLine(builder, language, $"DMS: {result.DmsText}", $"度分秒: {result.DmsText}");
        AppendLine(builder, language, $"Radians: {FormatNumber(result.Radians)}", $"弧度: {FormatNumber(result.Radians)}");
        AppendLine(
            builder,
            language,
            "Note: DMS precision is rounded for display only.",
            "说明: 度分秒精度仅用于显示时四舍五入。");
        AppendWarnings(builder, result.Warnings, language);
        return builder.ToString();
    }

    public string BuildTraverseQualityReport(
        TraverseQualityParseResult parseResult,
        TraverseQualityResult result,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Traverse Quality Report", "SurveyCalcKit 增强型闭合导线精度评价报告");
        if (!parseResult.IsSuccess)
        {
            AppendParseErrors(builder, parseResult.Errors, language);
            return builder.ToString();
        }

        AppendLine(builder, language, $"Point count: {result.PointCount}", $"点数: {result.PointCount}");
        AppendLine(builder, language, $"Segment count: {result.SegmentCount}", $"边数: {result.SegmentCount}");
        AppendLine(builder, language, $"Total length: {FormatNumber(result.TotalLength)}", $"导线总长: {FormatNumber(result.TotalLength)}");
        AppendLine(builder, language, $"fx: {FormatNumber(result.Fx)}", $"fx: {FormatNumber(result.Fx)}");
        AppendLine(builder, language, $"fy: {FormatNumber(result.Fy)}", $"fy: {FormatNumber(result.Fy)}");
        AppendLine(builder, language, $"Linear closure error: {FormatNumber(result.LinearClosureError)}", $"坐标闭合差: {FormatNumber(result.LinearClosureError)}");
        AppendLine(builder, language, $"Relative closure precision: {FormatRatio(result.RelativeClosureDenominator)}", $"相对闭合精度: {FormatRatio(result.RelativeClosureDenominator)}");
        AppendLine(builder, language, $"Linear closure limit: {FormatNullableBoolean(result.PassesLinearClosureLimit, language)}", $"平面闭合限差: {FormatNullableBoolean(result.PassesLinearClosureLimit, language)}");
        AppendLine(builder, language, $"Angular closure error seconds: {FormatNullable(result.AngularClosureErrorSeconds)}", $"角度闭合差(秒): {FormatNullable(result.AngularClosureErrorSeconds)}");
        AppendLine(builder, language, $"Allowable angular closure seconds: {FormatNullable(result.AllowableAngularClosureSeconds)}", $"角度闭合限差(秒): {FormatNullable(result.AllowableAngularClosureSeconds)}");
        AppendLine(builder, language, $"Angular closure limit: {FormatNullableBoolean(result.PassesAngularClosureLimit, language)}", $"角度闭合限差判定: {FormatNullableBoolean(result.PassesAngularClosureLimit, language)}");
        AppendLine(builder, language, $"Quality grade: {result.QualityGrade}", $"质量等级: {result.QualityGrade}");
        AppendTraverseQualityRows(builder, result.Segments, language);
        AppendWarnings(builder, result.Warnings, language);
        AppendParseErrors(builder, parseResult.Errors, language);
        return builder.ToString();
    }

    public string BuildCircularCurveReport(
        CircularCurveParseResult parseResult,
        CircularCurveResult result,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Circular Curve Report", "SurveyCalcKit 道路圆曲线要素计算报告");
        if (!parseResult.IsSuccess)
        {
            AppendParseErrors(builder, parseResult.Errors, language);
            return builder.ToString();
        }

        AppendLine(builder, language, $"Curve name: {result.CurveName}", $"曲线名: {result.CurveName}");
        AppendLine(builder, language, $"PI chainage: {FormatNumber(result.PiChainage)}", $"交点里程: {FormatNumber(result.PiChainage)}");
        AppendLine(builder, language, $"Radius: {FormatNumber(result.Radius)}", $"半径: {FormatNumber(result.Radius)}");
        AppendLine(builder, language, $"Deflection angle: {FormatNumber(result.DeflectionAngleDegrees)} degrees", $"转角: {FormatNumber(result.DeflectionAngleDegrees)} 度");
        AppendLine(builder, language, $"Direction: {result.TurnDirection}", $"转向: {result.TurnDirection}");
        AppendLine(builder, language, $"Tangent length T: {FormatNumber(result.TangentLength)}", $"切线长 T: {FormatNumber(result.TangentLength)}");
        AppendLine(builder, language, $"Curve length L: {FormatNumber(result.CurveLength)}", $"曲线长 L: {FormatNumber(result.CurveLength)}");
        AppendLine(builder, language, $"External distance E: {FormatNumber(result.ExternalDistance)}", $"外矢距 E: {FormatNumber(result.ExternalDistance)}");
        AppendLine(builder, language, $"Middle ordinate M: {FormatNumber(result.MiddleOrdinate)}", $"中矢距 M: {FormatNumber(result.MiddleOrdinate)}");
        AppendLine(builder, language, $"PC/ZY chainage: {FormatNumber(result.PcChainage)}", $"ZY/PC 里程: {FormatNumber(result.PcChainage)}");
        AppendLine(builder, language, $"PT/YZ chainage: {FormatNumber(result.PtChainage)}", $"YZ/PT 里程: {FormatNumber(result.PtChainage)}");
        AppendWarnings(builder, result.Warnings, language);
        AppendParseErrors(builder, parseResult.Errors, language);
        return builder.ToString();
    }

    public string BuildStakeoutBatchReport(
        StakeoutBatchParseResult parseResult,
        StakeoutBatchResult result,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Batch Stakeout Report", "SurveyCalcKit 批量放样点坐标计算报告");
        if (!parseResult.IsSuccess)
        {
            AppendParseErrors(builder, parseResult.Errors, language);
            return builder.ToString();
        }

        AppendLine(builder, language, $"Origin: {result.OriginPointName} ({FormatNumber(result.OriginX)}, {FormatNumber(result.OriginY)})", $"起算点: {result.OriginPointName} ({FormatNumber(result.OriginX)}, {FormatNumber(result.OriginY)})");
        AppendLine(builder, language, $"Baseline azimuth: {FormatNumber(result.BaselineAzimuthDegrees)} degrees", $"基线方位角: {FormatNumber(result.BaselineAzimuthDegrees)} 度");
        AppendLine(builder, language, $"Start chainage: {FormatNumber(result.StartChainage)}", $"起点里程: {FormatNumber(result.StartChainage)}");
        AppendStakeoutPointRows(builder, result.Points, language);
        AppendWarnings(builder, result.Warnings, language);
        AppendParseErrors(builder, parseResult.Errors, language);
        return builder.ToString();
    }

    public string BuildVerticalCurveReport(
        VerticalCurveParseResult parseResult,
        VerticalCurveResult result,
        ReportLanguage language = ReportLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        AppendTitle(builder, language, "SurveyCalcKit Vertical Curve Report", "SurveyCalcKit 竖曲线要素计算报告");
        if (!parseResult.IsSuccess)
        {
            AppendParseErrors(builder, parseResult.Errors, language);
            return builder.ToString();
        }

        AppendLine(builder, language, $"Curve name: {result.CurveName}", $"曲线名: {result.CurveName}");
        AppendLine(builder, language, $"PVI chainage: {FormatNumber(result.PviChainage)}", $"PVI 里程: {FormatNumber(result.PviChainage)}");
        AppendLine(builder, language, $"PVI elevation: {FormatNumber(result.PviElevation)}", $"PVI 高程: {FormatNumber(result.PviElevation)}");
        AppendLine(builder, language, $"Grade in: {FormatNumber(result.GradeInPercent)}%", $"进入坡度: {FormatNumber(result.GradeInPercent)}%");
        AppendLine(builder, language, $"Grade out: {FormatNumber(result.GradeOutPercent)}%", $"退出坡度: {FormatNumber(result.GradeOutPercent)}%");
        AppendLine(builder, language, $"Algebraic grade difference: {FormatNumber(result.AlgebraicGradeDifferencePercent)}%", $"坡度代数差: {FormatNumber(result.AlgebraicGradeDifferencePercent)}%");
        AppendLine(builder, language, $"Curve type: {result.CurveType}", $"曲线类型: {result.CurveType}");
        AppendLine(builder, language, $"Curve length: {FormatNumber(result.CurveLength)}", $"曲线长度: {FormatNumber(result.CurveLength)}");
        AppendLine(builder, language, $"PVC chainage/elevation: {FormatNumber(result.PvcChainage)} / {FormatNumber(result.PvcElevation)}", $"PVC 里程/高程: {FormatNumber(result.PvcChainage)} / {FormatNumber(result.PvcElevation)}");
        AppendLine(builder, language, $"PVT chainage/elevation: {FormatNumber(result.PvtChainage)} / {FormatNumber(result.PvtElevation)}", $"PVT 里程/高程: {FormatNumber(result.PvtChainage)} / {FormatNumber(result.PvtElevation)}");
        AppendVerticalCurvePointRows(builder, result.Points, language);
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

    private static void AppendBatchSegmentRows(
        StringBuilder builder,
        IReadOnlyList<BatchSegmentRow> rows,
        ReportLanguage language)
    {
        if (rows.Count == 0)
        {
            AppendLine(builder, language, "Segment rows: none", "线段行: 无");
            return;
        }

        AppendLine(builder, language, "Segment rows:", "线段表:");
        AppendLine(
            builder,
            language,
            "Index | From -> To | Dx | Dy | Distance2D | Azimuth | Cumulative | DeltaH | Slope%",
            "序号 | 起点 -> 终点 | Dx | Dy | 二维距离 | 方位角 | 累计距离 | 高差 | 坡度%");

        foreach (var row in rows)
        {
            builder.AppendLine(
                $"{row.Index} | {row.From} -> {row.To} | " +
                $"{FormatNumber(row.DeltaX)} | {FormatNumber(row.DeltaY)} | " +
                $"{FormatNumber(row.Distance2D)} | {FormatNumber(row.AzimuthDegrees)} | " +
                $"{FormatNumber(row.CumulativeDistance)} | {FormatNullable(row.DeltaH)} | " +
                $"{FormatNullable(row.SlopePercent)}");
        }
    }

    private static void AppendTraverseQualityRows(
        StringBuilder builder,
        IReadOnlyList<TraverseQualitySegmentRow> rows,
        ReportLanguage language)
    {
        if (rows.Count == 0)
        {
            AppendLine(builder, language, "Quality segment rows: none", "精度评价边表: 无");
            return;
        }

        AppendLine(builder, language, "Quality segment rows:", "精度评价边表:");
        AppendLine(
            builder,
            language,
            "Index | From -> To | Dx | Dy | Distance | Azimuth | Cumulative",
            "序号 | 起点 -> 终点 | Dx | Dy | 边长 | 方位角 | 累计长度");

        foreach (var row in rows)
        {
            builder.AppendLine(
                $"{row.Index} | {row.From} -> {row.To} | " +
                $"{FormatNumber(row.Dx)} | {FormatNumber(row.Dy)} | " +
                $"{FormatNumber(row.Distance)} | {FormatNumber(row.AzimuthDegrees)} | " +
                $"{FormatNumber(row.CumulativeLength)}");
        }
    }

    private static void AppendStakeoutPointRows(
        StringBuilder builder,
        IReadOnlyList<StakeoutPointResult> points,
        ReportLanguage language)
    {
        if (points.Count == 0)
        {
            AppendLine(builder, language, "Stakeout points: none", "放样点: 无");
            return;
        }

        AppendLine(builder, language, "Stakeout points:", "放样点坐标表:");
        AppendLine(
            builder,
            language,
            "Point | Chainage | Offset | Side | X | Y",
            "点名 | 里程 | 偏距 | 方向 | X | Y");

        foreach (var point in points)
        {
            builder.AppendLine(
                $"{point.PointName} | {FormatNumber(point.Chainage)} | {FormatNumber(point.Offset)} | " +
                $"{point.Side} | {FormatNumber(point.X)} | {FormatNumber(point.Y)}");
        }
    }

    private static void AppendVerticalCurvePointRows(
        StringBuilder builder,
        IReadOnlyList<VerticalCurvePointResult> points,
        ReportLanguage language)
    {
        if (points.Count == 0)
        {
            AppendLine(builder, language, "Design elevations: none", "设计高程: 无");
            return;
        }

        AppendLine(builder, language, "Design elevations:", "设计高程表:");
        AppendLine(
            builder,
            language,
            "Chainage | TangentElevation | CurveElevation | VerticalOffset | InsideCurve",
            "里程 | 切线高程 | 曲线高程 | 竖距 | 曲线范围内");

        foreach (var point in points)
        {
            builder.AppendLine(
                $"{FormatNumber(point.Chainage)} | {FormatNumber(point.TangentElevation)} | " +
                $"{FormatNumber(point.CurveElevation)} | {FormatNumber(point.VerticalOffset)} | " +
                $"{FormatBoolean(point.IsInsideCurve, language)}");
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

    private static string FormatNullableBoolean(bool? value, ReportLanguage language)
    {
        if (!value.HasValue)
        {
            return language == ReportLanguage.English ? "Not evaluated" : "未评价";
        }

        if (language == ReportLanguage.English)
        {
            return value.Value ? "Pass" : "Fail";
        }

        return value.Value ? "通过" : "不通过";
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
