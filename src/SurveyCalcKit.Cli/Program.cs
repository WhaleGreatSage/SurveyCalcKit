using System.Globalization;
using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

return SurveyCalcCli.Run(args);

internal static class SurveyCalcCli
{
    private static readonly ParseService Parser = new();
    private static readonly TraverseCalculator TraverseCalculator = new();
    private static readonly ClosedTraverseCalculator ClosedTraverseCalculator = new();
    private static readonly LevelingRouteCalculator LevelingRouteCalculator = new();
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
            "leveling" => RunLeveling(commandArgs),
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
              surveycalc leveling <file>
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
            """);
    }
}
