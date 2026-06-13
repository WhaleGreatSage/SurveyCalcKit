# Changelog

All notable changes to this project will be documented in this file.

## Unreleased

No unreleased changes.

## v0.2.2 - 2026-06-12

Added advanced engineering surveying modules: enhanced closed traverse quality evaluation, circular curve elements calculation, and batch stakeout point calculation. Includes Core services, parsers, CLI commands, WinForms buttons, samples, documentation, and unit tests.

## v0.1.6 - 2026-06-11

- Added coordinate inverse calculation with optional height and 3D distance support.
- Added batch segment table generation with cumulative distance and slope values.
- Added angle format conversion between decimal degrees, DMS text, and radians.
- Added Markdown report export for plain-text calculation reports.
- Added CLI `inverse`, `segments`, `angle`, and `export-md` commands.
- Added WinForms `Calculate Inverse`, `Calculate Segments`, `Convert Angle`, and `Export Markdown` actions.
- Added samples, documentation updates, and unit tests for the v0.1.6 feature set.

## v0.1.5 - 2026-06-07

- Added coordinate forward calculation from start point, azimuth, and distance.
- Added chainage and offset calculation against a straight baseline segment.
- Added CLI `forward` and `offset` commands.
- Added WinForms `Calculate Forward` and `Calculate Offset` actions.
- Added coordinate forward and chainage/offset samples.
- Added unit tests for new parsers and calculators.
- Updated README, usage guide, formulas, and roadmap documentation.

## v0.1.4 - 2026-06-05

- Added Excel `.xlsx` point import using ClosedXML.
- Added Excel export for traverse, leveling, polygon area, and report text workflows.
- Added CLI `import` and `export` commands.
- Added WinForms `导入 Excel` and `导出 Excel` actions.
- Added Excel sample workbook and unit tests.
- Updated Excel usage documentation.

## v0.2.1 - 2026-06-04

- Maintenance release for the v0.2 feature set.
- No code changes beyond changelog maintenance.
- Keeps closed traverse adjustment and leveling route adjustment as the current stable release features.

## v0.2.0 - 2026-06-03

- Added leveling route parser, closure error calculation, and station-count height adjustment.
- Added `surveycalc leveling <file>` CLI command.
- Added WinForms `Calculate Leveling` action.
- Added leveling route sample data and documentation.
- Added closed traverse closure calculation and Bowditch/Compass Rule adjustment.
- Added `surveycalc closure <file>` CLI command.
- Added WinForms `Calculate Closure` action.
- Added closed traverse sample data and documentation.

## v0.1.0 - 2026-06-01

- Initialized .NET 8 solution structure.
- Added `SurveyCalcKit.Core` models, parsing, traverse, elevation, coordinate transform, and report services.
- Added `SurveyCalcKit.Cli`.
- Added `SurveyCalcKit.WinForms`.
- Added xUnit tests for core beginner workflows.
- Added documentation, samples, and GitHub Actions workflow.
