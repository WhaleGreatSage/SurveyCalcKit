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
dotnet run --project src/SurveyCalcKit.Cli -- import samples/excel_sample.xlsx
dotnet run --project src/SurveyCalcKit.Cli -- export traverse traverse_results.xlsx --input samples/excel_sample.xlsx
dotnet run --project src/SurveyCalcKit.Cli -- traverse samples/traverse_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- elevation samples/elevation_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- closure samples/closed_traverse_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- leveling samples/leveling_route_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- forward samples/coordinate_forward_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- offset samples/chainage_offset_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- transform samples/transform_sample.txt --dx 500 --dy 1000 --scale 1.0002 --angle 15
```

Commands return `0` for valid input and non-zero for invalid input, missing files, or unsupported arguments.

## Excel Import and Export

Excel point import supports `.xlsx` files with a first-row header:

```text
Name | X | Y | H
```

`Name`, `X`, and `Y` are required. `H` is optional.

Import points from Excel:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- import samples/excel_sample.xlsx
```

Export calculation results to Excel:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- export traverse traverse_results.xlsx --input samples/excel_sample.xlsx
dotnet run --project src/SurveyCalcKit.Cli -- export leveling leveling_results.xlsx --input samples/leveling_route_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- export polygon polygon_area.xlsx --input samples/excel_sample.xlsx
```

For `traverse` and `polygon`, `--input` may point to a text point file or an Excel point file. For `leveling`, `--input` should use the leveling text route format.

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

## Leveling Route Adjustment

Use the `leveling` command for a route that starts and ends on known benchmarks:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- leveling samples/leveling_route_sample.txt
```

Input format:

```text
START BM1 100.000
P1 1.235 0.865
P2 1.120 0.940
P3 0.980 1.050
END BM2 100.480
```

The report includes:

- start and end benchmark names and elevations
- sum of backsight readings
- sum of foresight readings
- observed height difference
- known height difference
- closure error
- station count
- correction per station
- raw and adjusted elevations
- warnings for empty input, negative sight values, or large closure error

## Coordinate Forward Calculation

Use the `forward` command to calculate an endpoint from a start point, azimuth, and horizontal distance:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- forward samples/coordinate_forward_sample.txt
```

Input format:

```text
START P1 1000.000 1000.000
AZIMUTH 53.130102
DISTANCE 50.000
END P2
```

Comma-separated input is also supported:

```text
START,P1,1000.000,1000.000
AZIMUTH,53.130102
DISTANCE,50.000
END,P2
```

The report includes the normalized azimuth, distance, delta X, delta Y, endpoint name, endpoint coordinates, and warnings.

## Chainage and Offset Calculation

Use the `offset` command to calculate a target point's projected chainage and perpendicular offset relative to a straight baseline:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- offset samples/chainage_offset_sample.txt
```

Input format:

```text
BASELINE A 1000.000 1000.000 B 1100.000 1000.000
START_CHAINAGE 0.000
POINT P1 1050.000 1025.000
```

Comma-separated input is also supported:

```text
BASELINE,A,1000.000,1000.000,B,1100.000,1000.000
START_CHAINAGE,0.000
POINT,P1,1050.000,1025.000
```

The report includes baseline length, projection coordinates, chainage, offset, side, whether the projection is inside the segment, and warnings for weak input.

## WinForms

Run on Windows:

```bash
dotnet run --project src/SurveyCalcKit.WinForms
```

Paste or import point records into the left text box, then calculate traverse, elevation, leveling, closure, coordinate forward, or chainage/offset reports. Use `导入 Excel` to populate the input box from an Excel point workbook. Use `导出 Excel` to save the current report to an `.xlsx` workbook. Export saves the current report as a text file.
