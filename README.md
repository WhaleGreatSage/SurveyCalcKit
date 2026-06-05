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
- Calculate leveling route closure error and adjusted elevations.
- Calculate elevation closure error when known start and end elevations are available.
- Translate, scale, and rotate point coordinates.
- Import point data from Excel `.xlsx` files and export calculation results to Excel.
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
  excel_sample.xlsx
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
surveycalc leveling <file>
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
dotnet run --project src/SurveyCalcKit.Cli -- leveling samples/leveling_route_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- transform samples/transform_sample.txt --dx 500 --dy 1000 --scale 1.0002 --angle 15
```

Invalid input returns a non-zero exit code and prints clear row-level errors.

## WinForms Usage

The WinForms app provides:

- A left multiline text box for raw point input.
- A right multiline text box for calculation reports.
- Import, 导入 Excel, Calculate Traverse, Calculate Elevation, Calculate Leveling, Calculate Closure, 导出 Excel, Export Report, and Clear buttons.

Use `Import` to load `.txt`, `.dat`, or `.csv` style point files. Use `导入 Excel` to load `.xlsx` point data into the raw input box. Use the calculation buttons to generate a report. Use `Export Report` to save text output or `导出 Excel` to save the current report as an Excel workbook.

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
