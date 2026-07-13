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
dotnet run --project src/SurveyCalcKit.Cli -- quality samples/traverse_quality_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- leveling samples/leveling_route_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- curve samples/circular_curve_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- vertical-curve samples/vertical_curve_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- clothoid samples/clothoid_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- alignment-info samples/composite_alignment_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- alignment-query samples/composite_alignment_sample.txt samples/alignment_chainages_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- centerline-offset samples/centerline_offset_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- forward samples/coordinate_forward_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- inverse samples/coordinate_inverse_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- offset samples/chainage_offset_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- stakeout samples/stakeout_batch_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- segments samples/batch_segments_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- angle 53.130102
dotnet run --project src/SurveyCalcKit.Cli -- export-md samples/report_sample.txt output/report.md
dotnet run --project src/SurveyCalcKit.Cli -- export-dxf samples/dxf_points_sample.txt output/survey_points.dxf
dotnet run --project src/SurveyCalcKit.Cli -- import-geojson samples/points_sample.geojson
dotnet run --project src/SurveyCalcKit.Cli -- export-geojson line samples/dxf_points_sample.txt output/centerline.geojson
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

## Enhanced Closed Traverse Quality Evaluation

Use the `quality` command to evaluate a closed traverse with coordinate closure, relative closure precision, optional angular closure, allowable limits, and a quality grade:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- quality samples/traverse_quality_sample.txt
```

Input format:

```text
POINTS
P1 1000.000 1000.000
P2 1100.050 1002.200
P3 1098.600 1098.900
P4 998.900 1097.700
P1 1000.120 999.930
ANGLES
90.0020
89.9985
90.0040
89.9950
LIMITS
RELATIVE 2000
ANGULAR_SECONDS_PER_STATION 40
```

Comma-separated data rows are also accepted where reasonable, such as `P1,1000.000,1000.000` and `RELATIVE,2000`.

## Circular Curve Elements Calculation

Use the `curve` command for road circular curve element calculation:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- curve samples/circular_curve_sample.txt
```

Input format:

```text
CURVE C1
PI_CHAINAGE 1250.000
RADIUS 300.000
ANGLE 42.5000
DIRECTION RIGHT
```

Comma-separated key/value rows are also supported:

```text
CURVE,C1
PI_CHAINAGE,1250.000
RADIUS,300.000
ANGLE,42.5000
DIRECTION,RIGHT
```

The report includes tangent length, curve length, external distance, middle ordinate, and PC/ZY and PT/YZ chainages.

## Vertical Curve Calculation

Use the `vertical-curve` command for road profile vertical curve element calculation and design elevations:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- vertical-curve samples/vertical_curve_sample.txt
```

Input format:

```text
VERTICAL_CURVE VC1
PVI_CHAINAGE 1250.000
PVI_ELEVATION 56.800
GRADE_IN 2.000
GRADE_OUT -1.500
LENGTH 200.000
CHAINAGES
1150.000
1200.000
1250.000
1300.000
1350.000
```

Comma-separated rows are also supported where reasonable:

```text
VERTICAL_CURVE,VC1
PVI_CHAINAGE,1250.000
PVI_ELEVATION,56.800
GRADE_IN,2.000
GRADE_OUT,-1.500
LENGTH,200.000
CHAINAGE,1150.000
CHAINAGE,1200.000
```

The report includes the PVI chainage/elevation, incoming and outgoing grades, algebraic grade difference, crest/sag classification, curve length, PVC/PVT chainages and elevations, design elevation rows, and warnings for weak input.

## Batch Stakeout Point Calculation

Use the `stakeout` command to calculate multiple stakeout coordinates from a straight baseline direction:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- stakeout samples/stakeout_batch_sample.txt
```

Input format:

```text
ORIGIN A 1000.000 1000.000
AZIMUTH 35.0000
START_CHAINAGE 0.000
POINT K0+020 20.000 0.000
POINT K0+040_L5 40.000 5.000
POINT K0+060_R3 60.000 -3.000
```

Comma-separated input is also supported:

```text
ORIGIN,A,1000.000,1000.000
AZIMUTH,35.0000
START_CHAINAGE,0.000
POINT,K0+020,20.000,0.000
```

Positive offset means the left side of the baseline direction. Negative offset means the right side.

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

## Coordinate Inverse Calculation

Use the `inverse` command to calculate coordinate differences, 2D distance, azimuth, optional delta H, and optional 3D distance between two known points:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- inverse samples/coordinate_inverse_sample.txt
```

Input format:

```text
FROM P1 1000.000 1000.000 12.500
TO P2 1050.000 1040.000 13.200
```

Comma-separated input is also supported:

```text
FROM,P1,1000.000,1000.000,12.500
TO,P2,1050.000,1040.000,13.200
```

## Batch Segment Table

Use the `segments` command to generate a consecutive segment table with cumulative distance:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- segments samples/batch_segments_sample.txt
```

The input uses ordinary point rows:

```text
P1 1000.000 1000.000 12.500
P2 1030.000 1040.000 12.800
P3 1070.000 1060.000 13.100
P4 1100.000 1015.000 12.900
```

## Angle Format Converter

Use the `angle` command with either decimal degrees or DMS text:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- angle 53.130102
dotnet run --project src/SurveyCalcKit.Cli -- angle "53 7 48.37"
```

Supported DMS examples include:

```text
53°07'48.37"
53 7 48.37
53:7:48.37
```

DMS precision is rounded for display only; calculations use the underlying decimal degree value.

## Markdown Report Export

Use `export-md` to convert a plain text report into a UTF-8 Markdown file:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- export-md samples/report_sample.txt output/report.md
```

The Markdown document includes a title, generation timestamp, and fenced text block that preserves line breaks.

## DXF Export

Use `export-dxf` to write point records to a simple CAD-readable DXF file:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- export-dxf samples/dxf_points_sample.txt output/survey_points.dxf
```

Default export behavior writes POINT entities, TEXT point labels, and an open LWPOLYLINE through the point sequence. Optional flags:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- export-dxf samples/dxf_points_sample.txt output/survey_points.dxf --closed --layer SurveyCalcKit
dotnet run --project src/SurveyCalcKit.Cli -- export-dxf samples/dxf_points_sample.txt output/survey_points.dxf --no-labels
```

DXF export uses the existing point parser. It does not transform, scale, or adjust coordinates; it writes the parsed X/Y values directly as drawing coordinates.

## Clothoid Transition Curve

The clothoid command calculates a zero-to-arc transition with numerical coordinate integration:

~~~text
CLOTHOID S1
START 1000.000 1000.000
AZIMUTH 20.0000
RADIUS 300.000
LENGTH 80.000
DIRECTION RIGHT
DISTANCES
0
20
40
60
80
~~~

~~~bash
dotnet run --project src/SurveyCalcKit.Cli -- clothoid samples/clothoid_sample.txt
~~~

LEFT adds heading counterclockwise from the positive X axis; RIGHT subtracts it. Requested distances outside the spiral are clamped to the nearest endpoint and reported with a warning.

## Composite Alignment and Chainage Queries

The alignment parser connects each element from the computed end state of the previous one:

~~~text
ALIGNMENT Route-A
START_CHAINAGE 0.000
START 1000.000 1000.000
AZIMUTH 15.0000
ELEMENT TANGENT T1 LENGTH 100.000
ELEMENT CLOTHOID S1 LENGTH 60.000 RADIUS 300.000 DIRECTION LEFT
ELEMENT ARC C1 RADIUS 300.000 ANGLE 35.0000 DIRECTION LEFT
ELEMENT CLOTHOID S2 LENGTH 60.000 RADIUS 300.000 DIRECTION LEFT REVERSE
ELEMENT TANGENT T2 LENGTH 150.000
~~~

REVERSE on a clothoid changes curvature from the arc value back to zero. Query chainages are separate one-value-per-line files:

~~~bash
dotnet run --project src/SurveyCalcKit.Cli -- alignment-info samples/composite_alignment_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- alignment-query samples/composite_alignment_sample.txt samples/alignment_chainages_sample.txt
~~~

## Multi-Segment Centerline Offset

centerline-offset chooses the nearest projection over every consecutive centerline segment, then interpolates chainage and reports signed offset:

~~~bash
dotnet run --project src/SurveyCalcKit.Cli -- centerline-offset samples/centerline_offset_sample.txt
~~~

The input has CENTERLINE rows in Name Chainage X Y form and TARGETS rows in Name X Y form. Positive offset is left of the selected segment direction; negative offset is right.

## GeoJSON

Import accepts FeatureCollection Point, LineString, and Polygon exterior-ring coordinates. Export supports points, line, and polygon:

~~~bash
dotnet run --project src/SurveyCalcKit.Cli -- import-geojson samples/points_sample.geojson
dotnet run --project src/SurveyCalcKit.Cli -- export-geojson line samples/dxf_points_sample.txt output/centerline.geojson
~~~

GeoJSON coordinate order is [X, Y]. SurveyCalcKit preserves those values as raw planar coordinates and does not transform CRS or geographic longitude/latitude values.

## WinForms

The WinForms surface now includes `Calculate Vertical Curve` for vertical alignment reports and `Export DXF` for CAD-friendly point, label, and polyline output.

Run on Windows:

```bash
dotnet run --project src/SurveyCalcKit.WinForms
```

Paste or import records into the left text box, then calculate traverse, elevation, leveling, closure, quality evaluation, circular curve, stakeout, coordinate forward, coordinate inverse, batch segment, angle conversion, or chainage/offset reports. Use `导入 Excel` to populate the input box from an Excel point workbook. Use `导出 Excel` to save the current report to an `.xlsx` workbook. `Export Markdown` saves the current report as `.md`, and `Export Report` saves it as a text file.
