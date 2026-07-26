# Contributing

Thank you for considering a contribution to SurveyCalcKit.

## Development Setup

Install the .NET 8 SDK, then run:

```bash
dotnet restore
dotnet build
dotnet test
```

## Pull Request Guidelines

- Keep changes focused.
- Add or update tests for calculation behavior.
- Prefer readable code over clever code.
- Document formulas and conventions when adding surveying calculations.
- Do not add placeholder algorithms. If a formula is uncertain, open an issue first.

## Coding Style

- Use nullable reference types.
- Keep Core independent from UI concerns.
- Use `SurveyCalcKit.Core` for all calculation logic.
- Keep CLI and WinForms code as thin input/output layers.

## Route Alignment Modules

- Keep route geometry and numerical integration in Core services.
- Preserve the project azimuth convention: degrees counterclockwise from positive X.
- Add tolerance-based tests for floating-point geometry.
- Do not claim CRS transformation, survey accuracy classes, or design-standard compliance unless the implementation verifies them.

## Earthwork Modules

- Keep cut and fill quantities separate throughout calculation and reporting.
- Add hand-checkable tests whenever changing area or volume formulas.
- Document interpolation, sign, unit, and design-surface assumptions.
- Do not imply template, side-slope, shrink/swell, or regulatory support unless those behaviors are implemented and tested.

## Reporting Bugs

Please include:

- Input data that reproduces the issue.
- Expected result.
- Actual result.
- CLI command or UI steps used.
