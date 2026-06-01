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

## WinForms

Run on Windows:

```bash
dotnet run --project src/SurveyCalcKit.WinForms
```

Paste or import point records into the left text box, then calculate traverse or elevation reports. Export saves the current report as a text file.
