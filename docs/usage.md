# Usage Guide

SurveyCalcKit accepts simple point records. Each non-empty line must contain a point name, X, Y, and optionally H.

## Supported Input Formats

```text
P1 100.000 200.000
P1 100.000 200.000 15.230
P1,100.000,200.000
P1,100.000,200.000,15.230
```

Numbers are parsed with invariant culture, so use `.` as the decimal separator.

## CLI

Run from source:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- parse samples/traverse_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- traverse samples/traverse_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- elevation samples/elevation_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- closure samples/closed_traverse_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- transform samples/transform_sample.txt --dx 500 --dy 1000 --scale 1.0002 --angle 15
```

Commands return `0` for valid input and non-zero for invalid input, missing files, or unsupported arguments.

## Core Library

```csharp
using SurveyCalcKit.Core.Services;

var parser = new ParseService();
var traverse = new TraverseCalculator();
var reportBuilder = new ReportBuilder();

var parseResult = parser.ParsePoints(File.ReadAllText("samples/traverse_sample.txt"));
if (!parseResult.IsSuccess)
{
    Console.WriteLine(reportBuilder.BuildParseReport(parseResult));
    return;
}

var segments = traverse.CalculateSegments(parseResult.Points);
Console.WriteLine(reportBuilder.BuildTraverseReport(parseResult, segments));
```

## Closed Traverse Closure

Use the `closure` command when a traverse returns to its starting point:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- closure samples/closed_traverse_sample.txt
```

The first and last point should have the same name, or their coordinates should be very close. The report includes:

- coordinate closure components `fx` and `fy`
- linear closure error `f`
- total traverse length
- relative closure ratio
- Bowditch adjusted segment differences
- adjusted point coordinates
- warnings for weak or invalid input

Example input:

```text
P1 1000.000 1000.000 12.500
P2 1050.120 1001.350 12.760
P3 1048.900 1042.800 13.100
P4 998.750 1041.600 12.880
P1 1000.080 999.940 12.500
```

## WinForms

Run on Windows:

```bash
dotnet run --project src/SurveyCalcKit.WinForms
```

Paste or import point records into the left text box, then calculate traverse, elevation, or closure reports. Export saves the current report as a text file.
