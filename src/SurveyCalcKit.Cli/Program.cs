using System.Globalization;
using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

return SurveyCalcCli.Run(args);

internal static class SurveyCalcCli
{
    private static readonly ParseService Parser = new();
    private static readonly TraverseCalculator TraverseCalculator = new();
    private static readonly ClosedTraverseCalculator ClosedTraverseCalculator = new();
    private static readonly TraverseQualityEvaluator TraverseQualityEvaluator = new();
    private static readonly LevelingRouteCalculator LevelingRouteCalculator = new();
    private static readonly CoordinateForwardCalculator CoordinateForwardCalculator = new();
    private static readonly CoordinateInverseCalculator CoordinateInverseCalculator = new();
    private static readonly ChainageOffsetCalculator ChainageOffsetCalculator = new();
    private static readonly BatchSegmentTableCalculator BatchSegmentTableCalculator = new();
    private static readonly CircularCurveCalculator CircularCurveCalculator = new();
    private static readonly VerticalCurveCalculator VerticalCurveCalculator = new();
    private static readonly StakeoutBatchCalculator StakeoutBatchCalculator = new();
    private static readonly AngleConverter AngleConverter = new();
    private static readonly MarkdownReportExporter MarkdownReportExporter = new();
    private static readonly DxfExporter DxfExporter = new();
    private static readonly CoordinateTransformService TransformService = new();
    private static readonly ExcelService ExcelService = new();
    private static readonly ReportBuilder ReportBuilder = new();

    public static int Run(string[] args)
    {
        var commandArgs = StripExecutableName(args);
        if (commandArgs.Length == 0 || IsHelp(commandArgs[0]))
        {
            PrintUsage();
            return commandArgs.Length == 0 ? 1 : 0;
        }

        return commandArgs[0].ToLowerInvariant() switch
        {
            "parse" => RunParse(commandArgs),
            "import" => RunImport(commandArgs),
            "export" => RunExport(commandArgs),
            "traverse" => RunTraverse(commandArgs),
            "elevation" => RunElevation(commandArgs),
            "closure" => RunClosure(commandArgs),
            "quality" => RunQuality(commandArgs),
            "leveling" => RunLeveling(commandArgs),
            "curve" => RunCurve(commandArgs),
            "vertical-curve" => RunVerticalCurve(commandArgs),
            "forward" => RunForward(commandArgs),
            "inverse" => RunInverse(commandArgs),
            "offset" => RunOffset(commandArgs),
            "stakeout" => RunStakeout(commandArgs),
            "segments" => RunSegments(commandArgs),
            "angle" => RunAngle(commandArgs),
            "export-md" => RunExportMarkdown(commandArgs),
            "export-dxf" => RunExportDxf(commandArgs),
            "transform" => RunTransform(commandArgs),
            _ => Fail($"Unknown command '{commandArgs[0]}'.")
        };
    }

    private static int RunParse(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParsePoints(text);
        Console.WriteLine(ReportBuilder.BuildParseReport(parseResult));
        return parseResult.IsSuccess ? 0 : 1;
    }

    private static int RunImport(string[] args)
    {
        if (!TryReadFileArgument(args, out _, mustReadText: false))
        {
            return 1;
        }

        var result = ExcelService.ImportPoints(args[1]);
        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                Console.Error.WriteLine(error);
            }

            return 1;
        }

        foreach (var warning in result.Warnings)
        {
            Console.Error.WriteLine($"Warning: {warning}");
        }

        foreach (var point in result.Points)
        {
            Console.WriteLine(point.H.HasValue
                ? $"{point.Name} {FormatNumber(point.X)} {FormatNumber(point.Y)} {FormatNumber(point.H.Value)}"
                : $"{point.Name} {FormatNumber(point.X)} {FormatNumber(point.Y)}");
        }

        return 0;
    }

    private static int RunExport(string[] args)
    {
        if (args.Length < 3 || IsHelp(args[1]))
        {
            PrintUsage();
            return 1;
        }

        var resultType = args[1].ToLowerInvariant();
        var outputPath = args[2];
        var inputPath = ReadOption(args.Skip(3).ToArray(), "--input");
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return Fail("Export requires --input <data-file> so SurveyCalcKit can calculate results before writing Excel.");
        }

        ExcelExportResult exportResult;
        switch (resultType)
        {
            case "traverse":
                if (!TryLoadPointInput(inputPath, out var traversePoints))
                {
                    return 1;
                }

                exportResult = ExcelService.ExportTraverseResults(outputPath, TraverseCalculator.CalculateSegments(traversePoints));
                break;
            case "leveling":
                if (!TryLoadLevelingInput(inputPath, out var levelingInput))
                {
                    return 1;
                }

                exportResult = ExcelService.ExportLevelingResults(outputPath, LevelingRouteCalculator.Calculate(levelingInput));
                break;
            case "polygon":
                if (!TryLoadPointInput(inputPath, out var polygonPoints))
                {
                    return 1;
                }

                exportResult = ExcelService.ExportPolygonAreaResults(outputPath, polygonPoints, CalculatePolygonArea(polygonPoints));
                break;
            default:
                return Fail($"Unknown export result type '{resultType}'. Use traverse, leveling, or polygon.");
        }

        if (!exportResult.IsSuccess)
        {
            foreach (var error in exportResult.Errors)
            {
                Console.Error.WriteLine(error);
            }

            return 1;
        }

        Console.WriteLine($"Excel exported: {exportResult.FilePath}");
        return 0;
    }

    private static int RunTraverse(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParsePoints(text);
        if (!EnsureValidParseResult(parseResult))
        {
            return 1;
        }

        if (parseResult.Points.Count < 2)
        {
            return Fail("Traverse calculation requires at least two points.");
        }

        var segments = TraverseCalculator.CalculateSegments(parseResult.Points);
        Console.WriteLine(ReportBuilder.BuildTraverseReport(parseResult, segments));
        return 0;
    }

    private static int RunElevation(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParsePoints(text);
        if (!EnsureValidParseResult(parseResult))
        {
            return 1;
        }

        if (parseResult.Points.Count < 2)
        {
            return Fail("Elevation calculation requires at least two points.");
        }

        var segments = TraverseCalculator.CalculateSegments(parseResult.Points);
        Console.WriteLine(ReportBuilder.BuildElevationReport(parseResult, segments));
        return 0;
    }

    private static int RunClosure(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParsePoints(text);
        if (!EnsureValidParseResult(parseResult))
        {
            return 1;
        }

        var closureResult = ClosedTraverseCalculator.Calculate(parseResult.Points);
        Console.WriteLine(ReportBuilder.BuildClosureReport(parseResult, closureResult));

        return closureResult.AdjustedSegments.Count > 0 ? 0 : 1;
    }

    private static int RunQuality(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParseTraverseQuality(text);
        if (!parseResult.IsSuccess)
        {
            Console.Error.WriteLine(ReportBuilder.BuildTraverseQualityReport(
                parseResult,
                CreateEmptyTraverseQualityResult(),
                ReportLanguage.English));
            return 1;
        }

        var result = TraverseQualityEvaluator.Evaluate(parseResult.Input!);
        Console.WriteLine(ReportBuilder.BuildTraverseQualityReport(parseResult, result));
        return result.QualityGrade == "Failed" || result.QualityGrade == "NotEvaluated" ? 1 : 0;
    }

    private static int RunLeveling(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParseLevelingRoute(text);
        if (!parseResult.IsSuccess)
        {
            Console.Error.WriteLine(ReportBuilder.BuildLevelingParseReport(parseResult));
            return 1;
        }

        var routeResult = LevelingRouteCalculator.Calculate(parseResult.Route!);
        Console.WriteLine(ReportBuilder.BuildLevelingRouteReport(parseResult, routeResult));
        return routeResult.StationCount > 0 ? 0 : 1;
    }

    private static int RunCurve(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParseCircularCurve(text);
        if (!parseResult.IsSuccess)
        {
            Console.Error.WriteLine(ReportBuilder.BuildCircularCurveReport(
                parseResult,
                CreateEmptyCircularCurveResult(),
                ReportLanguage.English));
            return 1;
        }

        var result = CircularCurveCalculator.Calculate(parseResult.Input!);
        Console.WriteLine(ReportBuilder.BuildCircularCurveReport(parseResult, result));
        return result.Warnings.Count == 0 ? 0 : 1;
    }

    private static int RunVerticalCurve(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParseVerticalCurve(text);
        if (!parseResult.IsSuccess)
        {
            Console.Error.WriteLine(ReportBuilder.BuildVerticalCurveReport(
                parseResult,
                CreateEmptyVerticalCurveResult(),
                ReportLanguage.English));
            return 1;
        }

        var result = VerticalCurveCalculator.Calculate(parseResult.Input!);
        Console.WriteLine(ReportBuilder.BuildVerticalCurveReport(parseResult, result));
        return result.Points.Count > 0 ? 0 : 1;
    }

    private static int RunForward(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParseCoordinateForward(text);
        if (!parseResult.IsSuccess)
        {
            Console.Error.WriteLine(ReportBuilder.BuildCoordinateForwardReport(
                parseResult,
                CreateEmptyForwardResult(),
                ReportLanguage.English));
            return 1;
        }

        var result = CoordinateForwardCalculator.Calculate(parseResult.Input!);
        Console.WriteLine(ReportBuilder.BuildCoordinateForwardReport(parseResult, result));
        return 0;
    }

    private static int RunOffset(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParseChainageOffset(text);
        if (!parseResult.IsSuccess)
        {
            Console.Error.WriteLine(ReportBuilder.BuildChainageOffsetReport(
                parseResult,
                CreateEmptyChainageOffsetResult(),
                ReportLanguage.English));
            return 1;
        }

        var result = ChainageOffsetCalculator.Calculate(parseResult.Input!);
        Console.WriteLine(ReportBuilder.BuildChainageOffsetReport(parseResult, result));
        return result.BaselineLength > 0 ? 0 : 1;
    }

    private static int RunStakeout(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParseStakeoutBatch(text);
        if (!parseResult.IsSuccess)
        {
            Console.Error.WriteLine(ReportBuilder.BuildStakeoutBatchReport(
                parseResult,
                CreateEmptyStakeoutBatchResult(),
                ReportLanguage.English));
            return 1;
        }

        var result = StakeoutBatchCalculator.Calculate(parseResult.Input!);
        Console.WriteLine(ReportBuilder.BuildStakeoutBatchReport(parseResult, result));
        return result.Points.Count > 0 ? 0 : 1;
    }

    private static int RunInverse(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParseCoordinateInverse(text);
        if (!parseResult.IsSuccess)
        {
            Console.Error.WriteLine(ReportBuilder.BuildCoordinateInverseReport(
                parseResult,
                CreateEmptyInverseResult(),
                ReportLanguage.English));
            return 1;
        }

        var result = CoordinateInverseCalculator.Calculate(parseResult.Input!);
        Console.WriteLine(ReportBuilder.BuildCoordinateInverseReport(parseResult, result));
        return 0;
    }

    private static int RunSegments(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        var parseResult = Parser.ParsePoints(text);
        if (!EnsureValidParseResult(parseResult))
        {
            return 1;
        }

        var result = BatchSegmentTableCalculator.Calculate(parseResult.Points);
        Console.WriteLine(ReportBuilder.BuildBatchSegmentTableReport(parseResult, result));
        return result.SegmentCount > 0 ? 0 : 1;
    }

    private static int RunAngle(string[] args)
    {
        if (args.Length < 2 || IsHelp(args[1]))
        {
            PrintUsage();
            return 1;
        }

        var value = args[1];
        var input = TryParseDouble(value, out var decimalDegrees)
            ? new AngleConversionInput(decimalDegrees, null, null)
            : new AngleConversionInput(null, value, null);
        var result = AngleConverter.Convert(input);
        Console.WriteLine(ReportBuilder.BuildAngleConversionReport(result));

        return result.Warnings.Count == 0 ? 0 : 1;
    }

    private static int RunExportMarkdown(string[] args)
    {
        if (args.Length < 3 || IsHelp(args[1]))
        {
            PrintUsage();
            return 1;
        }

        var inputPath = args[1];
        var outputPath = args[2];
        if (!File.Exists(inputPath))
        {
            return Fail($"File not found: {inputPath}");
        }

        var reportText = File.ReadAllText(inputPath);
        var title = Path.GetFileNameWithoutExtension(inputPath);
        var result = MarkdownReportExporter.Export(title, reportText, outputPath);
        foreach (var warning in result.Warnings)
        {
            Console.Error.WriteLine($"Warning: {warning}");
        }

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                Console.Error.WriteLine(error);
            }

            return 1;
        }

        Console.WriteLine($"Markdown exported: {result.FilePath}");
        return 0;
    }

    private static int RunExportDxf(string[] args)
    {
        if (args.Length < 3 || IsHelp(args[1]))
        {
            PrintUsage();
            return 1;
        }

        var inputPath = args[1];
        var outputPath = args[2];
        if (!TryLoadPointInput(inputPath, out var points))
        {
            return 1;
        }

        if (!TryReadDxfOptions(args.Skip(3).ToArray(), out var options))
        {
            return 1;
        }

        var result = DxfExporter.Export(points, outputPath, options);
        foreach (var warning in result.Warnings)
        {
            Console.Error.WriteLine($"Warning: {warning}");
        }

        Console.WriteLine($"DXF exported: {result.OutputPath}");
        Console.WriteLine($"Point count: {result.PointCount}");
        Console.WriteLine($"Polyline exported: {result.PolylineExported}");
        return 0;
    }

    private static int RunTransform(string[] args)
    {
        if (!TryReadFileArgument(args, out var text))
        {
            return 1;
        }

        if (!TryReadTransformOptions(args.Skip(2).ToArray(), out var dx, out var dy, out var scale, out var angle))
        {
            return 1;
        }

        var parseResult = Parser.ParsePoints(text);
        if (!EnsureValidParseResult(parseResult))
        {
            return 1;
        }

        var transformedPoints = TransformService.Transform(parseResult.Points, dx, dy, scale, angle);
        Console.WriteLine(ReportBuilder.BuildTransformReport(parseResult, transformedPoints, dx, dy, scale, angle));
        return 0;
    }

    private static string[] StripExecutableName(string[] args)
    {
        return args.Length > 0 && string.Equals(args[0], "surveycalc", StringComparison.OrdinalIgnoreCase)
            ? args.Skip(1).ToArray()
            : args;
    }

    private static bool TryReadFileArgument(string[] args, out string text, bool mustReadText = true)
    {
        text = string.Empty;
        if (args.Length < 2 || IsHelp(args[1]))
        {
            PrintUsage();
            return false;
        }

        var path = args[1];
        if (!File.Exists(path))
        {
            Fail($"File not found: {path}");
            return false;
        }

        if (mustReadText)
        {
            text = File.ReadAllText(path);
        }

        return true;
    }

    private static bool TryLoadPointInput(string path, out IReadOnlyList<PointRecord> points)
    {
        points = Array.Empty<PointRecord>();
        if (!File.Exists(path))
        {
            Fail($"File not found: {path}");
            return false;
        }

        if (string.Equals(Path.GetExtension(path), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var excelResult = ExcelService.ImportPoints(path);
            if (!excelResult.IsSuccess)
            {
                foreach (var error in excelResult.Errors)
                {
                    Console.Error.WriteLine(error);
                }

                return false;
            }

            points = excelResult.Points;
            return true;
        }

        var parseResult = Parser.ParsePoints(File.ReadAllText(path));
        if (!EnsureValidParseResult(parseResult))
        {
            return false;
        }

        points = parseResult.Points;
        return true;
    }

    private static bool TryLoadLevelingInput(string path, out LevelingRouteInput input)
    {
        input = new LevelingRouteInput(string.Empty, 0, string.Empty, 0, new List<LevelingObservation>());
        if (!File.Exists(path))
        {
            Fail($"File not found: {path}");
            return false;
        }

        var parseResult = Parser.ParseLevelingRoute(File.ReadAllText(path));
        if (parseResult.IsSuccess)
        {
            input = parseResult.Route!;
            return true;
        }

        Console.Error.WriteLine(ReportBuilder.BuildLevelingParseReport(parseResult));
        return false;
    }

    private static bool TryReadTransformOptions(
        string[] args,
        out double dx,
        out double dy,
        out double scale,
        out double angle)
    {
        dx = 0;
        dy = 0;
        scale = 1;
        angle = 0;

        for (var i = 0; i < args.Length; i++)
        {
            var option = args[i];
            if (IsHelp(option))
            {
                PrintUsage();
                return false;
            }

            if (i + 1 >= args.Length)
            {
                Fail($"Missing value for option '{option}'.");
                return false;
            }

            var value = args[++i];
            if (!TryParseDouble(value, out var parsed))
            {
                Fail($"Invalid numeric value '{value}' for option '{option}'.");
                return false;
            }

            switch (option.ToLowerInvariant())
            {
                case "--dx":
                    dx = parsed;
                    break;
                case "--dy":
                    dy = parsed;
                    break;
                case "--scale":
                    scale = parsed;
                    break;
                case "--angle":
                    angle = parsed;
                    break;
                default:
                    Fail($"Unknown transform option '{option}'.");
                    return false;
            }
        }

        return true;
    }

    private static bool TryReadDxfOptions(string[] args, out DxfExportOptions options)
    {
        var layerName = "SurveyCalcKit";
        var exportLabels = true;
        var exportPolyline = true;
        var closePolyline = false;

        for (var i = 0; i < args.Length; i++)
        {
            var option = args[i];
            if (IsHelp(option))
            {
                PrintUsage();
                options = CreateDefaultDxfOptions();
                return false;
            }

            switch (option.ToLowerInvariant())
            {
                case "--no-labels":
                    exportLabels = false;
                    break;
                case "--polyline":
                    exportPolyline = true;
                    break;
                case "--closed":
                    closePolyline = true;
                    exportPolyline = true;
                    break;
                case "--layer":
                    if (i + 1 >= args.Length)
                    {
                        Fail("Missing value for --layer.");
                        options = CreateDefaultDxfOptions();
                        return false;
                    }

                    layerName = args[++i];
                    break;
                default:
                    Fail($"Unknown DXF option '{option}'.");
                    options = CreateDefaultDxfOptions();
                    return false;
            }
        }

        options = new DxfExportOptions(layerName, true, exportLabels, exportPolyline, closePolyline, 2.5);
        return true;
    }

    private static DxfExportOptions CreateDefaultDxfOptions()
    {
        return new DxfExportOptions("SurveyCalcKit", true, true, true, false, 2.5);
    }

    private static bool EnsureValidParseResult(ParseResult parseResult)
    {
        if (parseResult.IsSuccess)
        {
            return true;
        }

        Console.Error.WriteLine(ReportBuilder.BuildParseReport(parseResult));
        return false;
    }

    private static bool TryParseDouble(string value, out double result)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    private static string? ReadOption(string[] args, string optionName)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], optionName, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static double CalculatePolygonArea(IReadOnlyList<PointRecord> points)
    {
        if (points.Count < 3)
        {
            return 0;
        }

        var sum = 0.0;
        for (var i = 0; i < points.Count; i++)
        {
            var current = points[i];
            var next = points[(i + 1) % points.Count];
            sum += current.X * next.Y - next.X * current.Y;
        }

        return Math.Abs(sum) / 2.0;
    }

    private static CoordinateForwardResult CreateEmptyForwardResult()
    {
        return new CoordinateForwardResult(
            string.Empty,
            0,
            0,
            0,
            0,
            0,
            0,
            string.Empty,
            0,
            0,
            new List<string>());
    }

    private static ChainageOffsetResult CreateEmptyChainageOffsetResult()
    {
        return new ChainageOffsetResult(
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            0,
            0,
            0,
            "Undefined",
            false,
            0,
            0,
            new List<string>());
    }

    private static CoordinateInverseResult CreateEmptyInverseResult()
    {
        return new CoordinateInverseResult(
            string.Empty,
            string.Empty,
            0,
            0,
            0,
            0,
            null,
            null,
            new List<string>());
    }

    private static TraverseQualityResult CreateEmptyTraverseQualityResult()
    {
        return new TraverseQualityResult(
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            null,
            null,
            null,
            null,
            "NotEvaluated",
            new List<string>(),
            new List<TraverseQualitySegmentRow>());
    }

    private static CircularCurveResult CreateEmptyCircularCurveResult()
    {
        return new CircularCurveResult(
            string.Empty,
            0,
            0,
            0,
            string.Empty,
            0,
            0,
            0,
            0,
            0,
            0,
            new List<string>());
    }

    private static VerticalCurveResult CreateEmptyVerticalCurveResult()
    {
        return new VerticalCurveResult(
            string.Empty,
            0,
            0,
            0,
            0,
            0,
            0,
            "NotEvaluated",
            0,
            0,
            0,
            0,
            new List<VerticalCurvePointResult>(),
            new List<string>());
    }

    private static StakeoutBatchResult CreateEmptyStakeoutBatchResult()
    {
        return new StakeoutBatchResult(
            string.Empty,
            0,
            0,
            0,
            0,
            new List<StakeoutPointResult>(),
            new List<string>());
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static bool IsHelp(string value)
    {
        return value is "-h" or "--help" or "help";
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine(
            """
            SurveyCalcKit CLI

            Usage:
              surveycalc parse <file>
              surveycalc import <file.xlsx>
              surveycalc export <traverse|leveling|polygon> <file.xlsx> --input <data-file>
              surveycalc traverse <file>
              surveycalc elevation <file>
              surveycalc closure <file>
              surveycalc quality <file>
              surveycalc leveling <file>
              surveycalc curve <file>
              surveycalc vertical-curve <file>
              surveycalc forward <file>
              surveycalc inverse <file>
              surveycalc offset <file>
              surveycalc stakeout <file>
              surveycalc segments <file>
              surveycalc angle <value>
              surveycalc export-md <input-report.txt> <output.md>
              surveycalc export-dxf <input-points-file> <output-dxf-file> [--no-labels] [--polyline] [--closed] [--layer <name>]
              surveycalc transform <file> --dx <value> --dy <value> --scale <value> --angle <degrees>

            Input rows:
              P1 100.000 200.000
              P1 100.000 200.000 15.230
              P1,100.000,200.000
              P1,100.000,200.000,15.230

            Leveling rows:
              START BM1 100.000
              P1 1.235 0.865
              END BM2 100.480

            Coordinate forward rows:
              START P1 1000.000 1000.000
              AZIMUTH 53.130102
              DISTANCE 50.000
              END P2

            Coordinate inverse rows:
              FROM P1 1000.000 1000.000 12.500
              TO P2 1050.000 1040.000 13.200

            Chainage/offset rows:
              BASELINE A 1000.000 1000.000 B 1100.000 1000.000
              START_CHAINAGE 0.000
              POINT P1 1050.000 1025.000

            Traverse quality rows:
              POINTS
              P1 1000.000 1000.000
              ANGLES
              90.0000
              LIMITS
              RELATIVE 2000

            Circular curve rows:
              CURVE C1
              PI_CHAINAGE 1250.000
              RADIUS 300.000
              ANGLE 42.5000
              DIRECTION RIGHT

            Vertical curve rows:
              VERTICAL_CURVE VC1
              PVI_CHAINAGE 1250.000
              PVI_ELEVATION 56.800
              GRADE_IN 2.000
              GRADE_OUT -1.500
              LENGTH 200.000
              CHAINAGES
              1150.000
              1200.000

            Stakeout rows:
              ORIGIN A 1000.000 1000.000
              AZIMUTH 35.0000
              START_CHAINAGE 0.000
              POINT K0+020 20.000 0.000
            """);
    }
}
