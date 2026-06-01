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

## Reporting Bugs

Please include:

- Input data that reproduces the issue.
- Expected result.
- Actual result.
- CLI command or UI steps used.
