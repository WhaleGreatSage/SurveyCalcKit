# SurveyCalcKit

[![.NET](https://github.com/WhaleGreatSage/SurveyCalcKit/actions/workflows/dotnet.yml/badge.svg)](https://github.com/WhaleGreatSage/SurveyCalcKit/actions/workflows/dotnet.yml)

SurveyCalcKit is an open-source C#/.NET 8 surveying calculation toolkit for students, teachers, and beginner engineering surveying workflows.
## 中文说明

SurveyCalcKit 是一个面向测绘学生和初学者的开源测绘计算工具包，目标是用清晰、可测试的 C#/.NET 代码复现常见测绘计算流程，包括导线边长、方位角、高差、坡度、坐标变换和计算报告生成等功能。

这个项目也可以作为测绘程序设计、工程测量课程实践和 C# WinForms 入门练习的参考项目。

The project focuses on small, readable, testable calculations that can be used from a .NET class library, a command-line tool, or a simple WinForms desktop app.

## Who This Project Is For

- Surveying and civil engineering students learning coordinate and elevation calculations.
- Teachers who want transparent example code for classroom demonstrations.
- Beginners who need a small toolkit for parsing point lists, calculating traverse segments, checking slopes, and transforming coordinates.
- Contributors who want a maintainable .NET open-source project with tests and documentation.

## Features

- Parse whitespace or comma-separated point records.
- Support optional elevation values.
- Calculate segment dx, dy, 2D distance, optional 3D distance, azimuth, delta H, and slope percentage.
- Calculate total 2D traverse length.
- Calculate closed traverse coordinate closure error and Bowditch/Compass Rule adjustment.
- Evaluate enhanced closed traverse quality with relative precision, angular closure checks, limits, and quality grades.
- Calculate leveling route closure error and adjusted elevations.
- Calculate circular curve elements for road and route engineering.
- Calculate vertical curve elements and design elevations for road profile work.
- Calculate batch stakeout point coordinates from origin, azimuth, chainage, and offset.
- Calculate endpoint coordinates from start point, azimuth, and horizontal distance.
- Calculate inverse coordinate values between two known points.
- Generate batch segment tables with cumulative distance.
- Convert angles between decimal degrees, DMS text, and radians.
- Calculate chainage, perpendicular offset, side, and projection point relative to a baseline.
- Calculate elevation closure error when known start and end elevations are available.
- Translate, scale, and rotate point coordinates.
- Import point data from Excel `.xlsx` files and export calculation results to Excel.
- Export plain-text reports to UTF-8 Markdown files.
- Export point records, point labels, and connected polylines to simple DXF files.
- Generate readable English and Chinese reports.
- Use the same core calculation library from CLI and WinForms.
- Run automated .NET restore, build, and test checks through GitHub Actions.

## Repository Structure

```text
SurveyCalcKit.sln
src/
  SurveyCalcKit.Core/       Core models, parsers, calculators, reports
  SurveyCalcKit.Cli/        Command-line interface
  SurveyCalcKit.WinForms/   Simple Windows desktop interface
tests/
  SurveyCalcKit.Tests/      xUnit tests for core behavior
docs/
  usage.md                  Usage guide
  formulas.md               Calculation formulas and conventions
  roadmap.md                Planned project direction
samples/
  traverse_sample.txt
  elevation_sample.txt
  transform_sample.txt
  closed_traverse_sample.txt
  leveling_route_sample.txt
  coordinate_forward_sample.txt
  coordinate_inverse_sample.txt
  batch_segments_sample.txt
  chainage_offset_sample.txt
  traverse_quality_sample.txt
  circular_curve_sample.txt
  vertical_curve_sample.txt
  stakeout_batch_sample.txt
  dxf_points_sample.txt
  report_sample.txt
  excel_sample.xlsx
output/
  .gitkeep
.github/workflows/
  dotnet.yml                CI workflow
```

## Quick Start

Install the .NET 8 SDK, then run:

```bash
dotnet restore
dotnet build
dotnet test
```

Run the CLI from source:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- traverse samples/traverse_sample.txt
```

Run the WinForms app on Windows:

```bash
dotnet run --project src/SurveyCalcKit.WinForms
```

## CLI Usage

The CLI executable is named `surveycalc` when published or built.

```bash
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
```

From source, place the command after `--`:

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
dotnet run --project src/SurveyCalcKit.Cli -- forward samples/coordinate_forward_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- inverse samples/coordinate_inverse_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- offset samples/chainage_offset_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- stakeout samples/stakeout_batch_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- segments samples/batch_segments_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- angle 53.130102
dotnet run --project src/SurveyCalcKit.Cli -- export-md samples/report_sample.txt output/report.md
dotnet run --project src/SurveyCalcKit.Cli -- export-dxf samples/dxf_points_sample.txt output/survey_points.dxf
dotnet run --project src/SurveyCalcKit.Cli -- transform samples/transform_sample.txt --dx 500 --dy 1000 --scale 1.0002 --angle 15
```

Invalid input returns a non-zero exit code and prints clear row-level errors.

## WinForms Usage

The WinForms app provides:

- A left multiline text box for raw point input.
- A right multiline text box for calculation reports.
- Buttons include `Calculate Vertical Curve` for profile design elevations and `Export DXF` for CAD-friendly point and polyline output.
- Import, 导入 Excel, Calculate Traverse, Calculate Elevation, Calculate Leveling, Calculate Closure, Evaluate Quality, Calculate Curve, Calculate Forward, Calculate Offset, Calculate Stakeout, Calculate Inverse, Calculate Segments, Convert Angle, 导出 Excel, Export Markdown, Export Report, and Clear buttons.

Use `Import` to load `.txt`, `.dat`, or `.csv` style point files. Use `导入 Excel` to load `.xlsx` point data into the raw input box. Use the calculation buttons to generate traverse, elevation, leveling, closure, quality evaluation, circular curve, stakeout, coordinate forward, coordinate inverse, batch segment, angle conversion, or chainage/offset reports. Use `Export Report`, `Export Markdown`, or `导出 Excel` to save the current report.

For DXF export in WinForms, paste or import point rows in the left text box, click `Export DXF`, choose a `.dxf` path, and inspect the saved POINT, TEXT label, and polyline entities in CAD software.

## Sample Input

```text
P1 1000.000 1000.000 12.500
P2 1030.000 1040.000 13.200
P3 1070.000 1025.000 13.550
```

Comma-separated input is also supported:

```text
P1,1000.000,1000.000,12.500
P2,1030.000,1040.000,13.200
```

## Sample Output

```text
SurveyCalcKit Traverse Report
=============================
Parsed point count: 3
Segments:
From -> To | Dx | Dy | Distance2D | Distance3D | Azimuth | DeltaH | Slope%
P1 -> P2 | 30 | 40 | 50 | 50.005 | 53.13 | 0.7 | 1.4
P2 -> P3 | 40 | -15 | 42.72 | 42.721 | 339.444 | 0.35 | 0.819
Total 2D length: 92.72
Warnings: none
```

## Closed Traverse Closure and Bowditch Adjustment

Closed traverse input should include the starting point again as the final row, either with the same name or coordinates close to the start:

```text
P1 1000.000 1000.000 12.500
P2 1050.120 1001.350 12.760
P3 1048.900 1042.800 13.100
P4 998.750 1041.600 12.880
P1 1000.080 999.940 12.500
```

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- closure samples/closed_traverse_sample.txt
```

Sample output includes closure components and adjusted coordinates:

```text
Closure fx=0.08, fy=-0.06, f=0.1
Total length: 183.452
Relative closure ratio: 1:1834.517
Bowditch adjusted segments:
Adjusted coordinates:
Warnings: none
```

Bowditch adjustment distributes the coordinate closure error to each segment in proportion to segment length. This is a simple classroom-friendly adjustment method; it assumes comparable observation quality and does not replace a full least-squares network adjustment.

## Leveling Route Adjustment

A leveling route starts from a known benchmark, uses backsight and foresight readings at each station, and closes on another known benchmark:

```text
START BM1 100.000
P1 1.235 0.865
P2 1.120 0.940
P3 0.980 1.050
END BM2 100.480
```

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- leveling samples/leveling_route_sample.txt
```

Sample output includes the backsight/foresight sums, observed and known height differences, closure error, per-station correction, and adjusted elevations:

```text
Sum backsight: 3.335
Sum foresight: 2.855
Observed height difference: 0.48
Known height difference: 0.48
Closure error: 0
Correction per station: 0
Adjusted elevations:
```

The simple adjustment method distributes closure error equally by station count. It is suitable for beginner checking workflows, but it does not model sight length, instrument precision, or weighted least-squares adjustment.

## Enhanced Closed Traverse Quality Evaluation / 增强型闭合导线精度评价

The `quality` command evaluates a closed traverse using coordinate closure, relative closure precision, optional angular closure error, allowable limits, and a simple quality grade.

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

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- quality samples/traverse_quality_sample.txt
```

The report includes `fx`, `fy`, linear closure error, relative closure precision, angular closure error in seconds, pass/fail checks, quality grade, segment rows, and warnings. The angular closure check uses the theoretical interior angle sum `(n - 2) * 180` for a closed traverse.

## Circular Curve Elements Calculation / 道路圆曲线要素计算

The `curve` command calculates basic circular curve elements from PI chainage, radius, deflection angle, and turn direction.

```text
CURVE C1
PI_CHAINAGE 1250.000
RADIUS 300.000
ANGLE 42.5000
DIRECTION RIGHT
```

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- curve samples/circular_curve_sample.txt
```

The report includes tangent length `T`, curve length `L`, external distance `E`, middle ordinate `M`, and PC/ZY and PT/YZ chainages. Units are assumed to be meters and degrees.

## Vertical Curve Calculation / Vertical Profile Design

The `vertical-curve` command calculates PVC/PVT chainages, PVC/PVT elevations, curve type, and design elevations at requested chainages from a PVI, incoming grade, outgoing grade, and curve length.

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

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- vertical-curve samples/vertical_curve_sample.txt
```

Sample output includes the algebraic grade difference, crest/sag classification, PVC/PVT chainage and elevation, and a design elevation table. Chainages outside the curve are allowed but are marked as outside and reported with warnings.

## DXF Export / CAD Output

The `export-dxf` command writes parsed point records to a simple UTF-8 DXF file with POINT entities, optional TEXT labels, and a connected LWPOLYLINE. Coordinates are written as-is in drawing units, so this export does not transform or adjust point coordinates.

```text
P1 1000.000 1000.000
P2 1050.000 1000.000
P3 1050.000 1040.000
P4 1000.000 1040.000
```

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- export-dxf samples/dxf_points_sample.txt output/survey_points.dxf
```

Optional flags include `--no-labels`, `--polyline`, `--closed`, and `--layer <name>`. The default behavior exports points, labels, and an open polyline.

## Batch Stakeout Point Calculation / 批量放样点坐标计算

The `stakeout` command calculates multiple stakeout coordinates from an origin point, baseline azimuth, start chainage, and chainage/offset records. SurveyCalcKit uses the same angle convention as traverse azimuth: degrees counterclockwise from the positive X axis. Positive offset is the left side of the baseline direction; negative offset is the right side.

```text
ORIGIN A 1000.000 1000.000
AZIMUTH 35.0000
START_CHAINAGE 0.000
POINT K0+020 20.000 0.000
POINT K0+040_L5 40.000 5.000
POINT K0+060_R3 60.000 -3.000
```

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- stakeout samples/stakeout_batch_sample.txt
```

The report includes point name, chainage, offset, side, and calculated X/Y coordinates. The method is intended for straight baseline layout workflows; curved alignments and spiral transition curves are future work.

## Coordinate Forward Calculation / 坐标正算

Coordinate forward calculation computes an endpoint from a known start coordinate, azimuth angle, and horizontal distance. SurveyCalcKit uses the same angle convention as its traverse azimuth: degrees measured counterclockwise from the positive X axis, normalized to `0` through `360`.

```text
START P1 1000.000 1000.000
AZIMUTH 53.130102
DISTANCE 50.000
END P2
```

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- forward samples/coordinate_forward_sample.txt
```

Sample output:

```text
Delta X: 30
Delta Y: 40
End point: P2
End coordinates: X=1030, Y=1040
Warnings: none
```

## Chainage and Offset Calculation / 里程与偏距计算

Chainage and offset calculation projects a target point onto a baseline segment, then reports the distance along the baseline, perpendicular offset, left/right/on-line side, projection coordinate, and whether the projection falls inside the segment.

```text
BASELINE A 1000.000 1000.000 B 1100.000 1000.000
START_CHAINAGE 0.000
POINT P1 1050.000 1025.000
```

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- offset samples/chainage_offset_sample.txt
```

Sample output:

```text
Baseline length: 100
Projection coordinates: X=1050, Y=1000
Chainage: 50
Offset: 25
Side: Left
Projection inside segment: Yes
```

This straight-segment method is suitable for simple baseline checks and beginner stakeout exercises. It does not yet calculate chainage along circular curves, transition curves, or multi-segment centerlines.

## Coordinate Inverse Calculation / 坐标反算

Coordinate inverse calculation computes `Delta X`, `Delta Y`, 2D distance, azimuth, optional elevation difference, and optional 3D distance from two known point coordinates.

```text
FROM P1 1000.000 1000.000 12.500
TO P2 1050.000 1040.000 13.200
```

Run:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- inverse samples/coordinate_inverse_sample.txt
```

Sample output:

```text
Delta X: 50
Delta Y: 40
Distance 2D: 64.031
Azimuth: 38.66 degrees
Delta H: 0.7
Distance 3D: 64.035
```

## Batch Segment Table / 批量边长方位角表

Batch segment table generation turns a point list into consecutive segment rows with distance, azimuth, cumulative distance, optional delta H, and optional slope percentage.

```bash
dotnet run --project src/SurveyCalcKit.Cli -- segments samples/batch_segments_sample.txt
```

Sample output includes:

```text
Parsed point count: 4
Segment count: 3
Total length: 148.805
1 | P1 -> P2 | 30 | 40 | 50 | 53.13 | 50 | 0.3 | 0.6
```

## Angle Format Converter / 角度格式转换

The angle converter accepts decimal degrees or DMS text such as `53°07'48.37"`, `53 7 48.37`, or `53:7:48.37`, then reports decimal degrees, DMS, and radians. DMS precision is rounded for display only.

```bash
dotnet run --project src/SurveyCalcKit.Cli -- angle 53.130102
```

Sample output:

```text
Decimal degrees: 53.13
DMS: 53°07'48.37"
Radians: 0.927
```

## Markdown Report Export / Markdown 报告导出

Markdown export converts an existing plain-text report into a UTF-8 `.md` file with a title, generation timestamp, and fenced text block that preserves line breaks.

```bash
dotnet run --project src/SurveyCalcKit.Cli -- export-md samples/report_sample.txt output/report.md
```

## Excel Import and Export

Point import expects a `.xlsx` worksheet with these headers in the first row:

```text
Name | X | Y | H
```

`H` is optional. Example CLI usage:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- import samples/excel_sample.xlsx
dotnet run --project src/SurveyCalcKit.Cli -- export traverse traverse_results.xlsx --input samples/excel_sample.xlsx
dotnet run --project src/SurveyCalcKit.Cli -- export leveling leveling_results.xlsx --input samples/leveling_route_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- export polygon polygon_area.xlsx --input samples/excel_sample.xlsx
```

The Excel exporter writes structured worksheets for traverse segment results, leveling summaries and adjusted elevations, polygon area summaries, point tables, and WinForms report text.

## Roadmap

- Add more traverse adjustment workflows.
- Add import/export helpers for common classroom spreadsheet formats.
- Add richer WinForms workflows for coordinate transformation.
- Add NuGet packaging for `SurveyCalcKit.Core`.
- Add localized report templates and examples.

See [docs/roadmap.md](docs/roadmap.md) for details.

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening issues or pull requests.

## License

SurveyCalcKit is licensed under the MIT License. See [LICENSE](LICENSE).
