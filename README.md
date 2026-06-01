# SurveyCalcKit

SurveyCalcKit is an open-source C#/.NET 8 surveying calculation toolkit for students, teachers, and beginner engineering surveying workflows.

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
- Calculate elevation closure error when known start and end elevations are available.
- Translate, scale, and rotate point coordinates.
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
surveycalc traverse <file>
surveycalc elevation <file>
surveycalc transform <file> --dx <value> --dy <value> --scale <value> --angle <degrees>
```

From source, place the command after `--`:

```bash
dotnet run --project src/SurveyCalcKit.Cli -- parse samples/traverse_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- traverse samples/traverse_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- elevation samples/elevation_sample.txt
dotnet run --project src/SurveyCalcKit.Cli -- transform samples/transform_sample.txt --dx 500 --dy 1000 --scale 1.0002 --angle 15
```

Invalid input returns a non-zero exit code and prints clear row-level errors.

## WinForms Usage

The WinForms app provides:

- A left multiline text box for raw point input.
- A right multiline text box for calculation reports.
- Import, Calculate Traverse, Calculate Elevation, Export Report, and Clear buttons.

Use `Import` to load `.txt`, `.dat`, or `.csv` style point files. Use the calculation buttons to generate a report. Use `Export Report` to save the output as text.

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
