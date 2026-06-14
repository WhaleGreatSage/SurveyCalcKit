using System.Globalization;
using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.WinForms;

public partial class Form1 : Form
{
    private readonly ParseService parseService = new();
    private readonly TraverseCalculator traverseCalculator = new();
    private readonly ClosedTraverseCalculator closedTraverseCalculator = new();
    private readonly TraverseQualityEvaluator traverseQualityEvaluator = new();
    private readonly LevelingRouteCalculator levelingRouteCalculator = new();
    private readonly CircularCurveCalculator circularCurveCalculator = new();
    private readonly VerticalCurveCalculator verticalCurveCalculator = new();
    private readonly CoordinateForwardCalculator coordinateForwardCalculator = new();
    private readonly CoordinateInverseCalculator coordinateInverseCalculator = new();
    private readonly ChainageOffsetCalculator chainageOffsetCalculator = new();
    private readonly StakeoutBatchCalculator stakeoutBatchCalculator = new();
    private readonly BatchSegmentTableCalculator batchSegmentTableCalculator = new();
    private readonly AngleConverter angleConverter = new();
    private readonly MarkdownReportExporter markdownReportExporter = new();
    private readonly DxfExporter dxfExporter = new();
    private readonly ExcelService excelService = new();
    private readonly ReportBuilder reportBuilder = new();

    public Form1()
    {
        InitializeComponent();
    }

    private void ImportButton_Click(object? sender, EventArgs e)
    {
        if (openFileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            rawInputTextBox.Text = File.ReadAllText(openFileDialog.FileName);
            reportOutputTextBox.Clear();
        }
        catch (IOException ex)
        {
            ShowError($"Could not import file: {ex.Message}");
        }
    }

    private void ImportExcelButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Excel workbook (*.xlsx)|*.xlsx|All files (*.*)|*.*",
            Title = "Import Excel point data"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = excelService.ImportPoints(dialog.FileName);
        if (!result.IsSuccess)
        {
            reportOutputTextBox.Text = string.Join(Environment.NewLine, result.Errors);
            return;
        }

        rawInputTextBox.Text = string.Join(Environment.NewLine, result.Points.Select(FormatPointForInput));
        reportOutputTextBox.Text = $"Imported {result.Points.Count} point(s) from Excel.";
    }

    private void CalculateTraverseButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParsePoints(rawInputTextBox.Text);
        if (!TryShowParseErrors(parseResult))
        {
            return;
        }

        if (parseResult.Points.Count < 2)
        {
            ShowError("Traverse calculation requires at least two points.");
            return;
        }

        var segments = traverseCalculator.CalculateSegments(parseResult.Points);
        reportOutputTextBox.Text = reportBuilder.BuildTraverseReport(parseResult, segments, ReportLanguage.English);
    }

    private void CalculateElevationButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParsePoints(rawInputTextBox.Text);
        if (!TryShowParseErrors(parseResult))
        {
            return;
        }

        if (parseResult.Points.Count < 2)
        {
            ShowError("Elevation calculation requires at least two points.");
            return;
        }

        var segments = traverseCalculator.CalculateSegments(parseResult.Points);
        reportOutputTextBox.Text = reportBuilder.BuildElevationReport(parseResult, segments, language: ReportLanguage.English);
    }

    private void CalculateLevelingButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseLevelingRoute(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildLevelingParseReport(parseResult, ReportLanguage.English);
            return;
        }

        var levelingResult = levelingRouteCalculator.Calculate(parseResult.Route!);
        reportOutputTextBox.Text = reportBuilder.BuildLevelingRouteReport(parseResult, levelingResult, ReportLanguage.English);
    }

    private void CalculateClosureButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParsePoints(rawInputTextBox.Text);
        if (!TryShowParseErrors(parseResult))
        {
            return;
        }

        var closureResult = closedTraverseCalculator.Calculate(parseResult.Points);
        reportOutputTextBox.Text = reportBuilder.BuildClosureReport(parseResult, closureResult, ReportLanguage.English);
    }

    private void EvaluateQualityButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseTraverseQuality(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildTraverseQualityReport(
                parseResult,
                CreateEmptyTraverseQualityResult(),
                ReportLanguage.English);
            return;
        }

        var result = traverseQualityEvaluator.Evaluate(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildTraverseQualityReport(parseResult, result, ReportLanguage.English);
    }

    private void CalculateCurveButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseCircularCurve(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildCircularCurveReport(
                parseResult,
                CreateEmptyCircularCurveResult(),
                ReportLanguage.English);
            return;
        }

        var result = circularCurveCalculator.Calculate(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildCircularCurveReport(parseResult, result, ReportLanguage.English);
    }

    private void CalculateVerticalCurveButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseVerticalCurve(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildVerticalCurveReport(
                parseResult,
                CreateEmptyVerticalCurveResult(),
                ReportLanguage.English);
            return;
        }

        var result = verticalCurveCalculator.Calculate(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildVerticalCurveReport(parseResult, result, ReportLanguage.English);
    }

    private void CalculateForwardButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseCoordinateForward(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildCoordinateForwardReport(
                parseResult,
                CreateEmptyForwardResult(),
                ReportLanguage.English);
            return;
        }

        var result = coordinateForwardCalculator.Calculate(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildCoordinateForwardReport(parseResult, result, ReportLanguage.English);
    }

    private void CalculateOffsetButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseChainageOffset(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildChainageOffsetReport(
                parseResult,
                CreateEmptyChainageOffsetResult(),
                ReportLanguage.English);
            return;
        }

        var result = chainageOffsetCalculator.Calculate(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildChainageOffsetReport(parseResult, result, ReportLanguage.English);
    }

    private void CalculateStakeoutButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseStakeoutBatch(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildStakeoutBatchReport(
                parseResult,
                CreateEmptyStakeoutBatchResult(),
                ReportLanguage.English);
            return;
        }

        var result = stakeoutBatchCalculator.Calculate(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildStakeoutBatchReport(parseResult, result, ReportLanguage.English);
    }

    private void CalculateInverseButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseCoordinateInverse(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildCoordinateInverseReport(
                parseResult,
                CreateEmptyInverseResult(),
                ReportLanguage.English);
            return;
        }

        var result = coordinateInverseCalculator.Calculate(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildCoordinateInverseReport(parseResult, result, ReportLanguage.English);
    }

    private void CalculateSegmentsButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParsePoints(rawInputTextBox.Text);
        if (!TryShowParseErrors(parseResult))
        {
            return;
        }

        var result = batchSegmentTableCalculator.Calculate(parseResult.Points);
        reportOutputTextBox.Text = reportBuilder.BuildBatchSegmentTableReport(parseResult, result, ReportLanguage.English);
    }

    private void ConvertAngleButton_Click(object? sender, EventArgs e)
    {
        var value = rawInputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            ShowError("Enter a decimal degree or DMS angle value in the raw input box.");
            return;
        }

        var input = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalDegrees)
            ? new AngleConversionInput(decimalDegrees, null, null)
            : new AngleConversionInput(null, value, null);
        var result = angleConverter.Convert(input);
        reportOutputTextBox.Text = reportBuilder.BuildAngleConversionReport(result, ReportLanguage.English);
    }

    private void ExportReportButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(reportOutputTextBox.Text))
        {
            ShowError("There is no report to export.");
            return;
        }

        if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            File.WriteAllText(saveFileDialog.FileName, reportOutputTextBox.Text);
        }
        catch (IOException ex)
        {
            ShowError($"Could not export report: {ex.Message}");
        }
    }

    private void ExportMarkdownButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(reportOutputTextBox.Text))
        {
            ShowError("There is no report to export.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            DefaultExt = "md",
            Filter = "Markdown report (*.md)|*.md|All files (*.*)|*.*",
            Title = "Export Markdown report"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = markdownReportExporter.Export("SurveyCalcKit Report", reportOutputTextBox.Text, dialog.FileName);
        if (!result.IsSuccess)
        {
            ShowError(string.Join(Environment.NewLine, result.Errors.Concat(result.Warnings)));
        }
    }

    private void ExportExcelButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(reportOutputTextBox.Text))
        {
            ShowError("There is no report to export.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            DefaultExt = "xlsx",
            Filter = "Excel workbook (*.xlsx)|*.xlsx|All files (*.*)|*.*",
            Title = "Export report to Excel"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = excelService.ExportReportText(dialog.FileName, "SurveyCalcKit Report", reportOutputTextBox.Text);
        if (!result.IsSuccess)
        {
            ShowError(string.Join(Environment.NewLine, result.Errors));
        }
    }

    private void ExportDxfButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParsePoints(rawInputTextBox.Text);
        if (!TryShowParseErrors(parseResult))
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            DefaultExt = "dxf",
            Filter = "DXF drawing (*.dxf)|*.dxf|All files (*.*)|*.*",
            Title = "Export DXF"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = dxfExporter.Export(
            parseResult.Points,
            dialog.FileName,
            new DxfExportOptions("SurveyCalcKit", true, true, true, false, 2.5));
        reportOutputTextBox.Text =
            $"DXF exported: {result.OutputPath}{Environment.NewLine}" +
            $"Point count: {result.PointCount}{Environment.NewLine}" +
            $"Polyline exported: {result.PolylineExported}{Environment.NewLine}" +
            (result.Warnings.Count == 0
                ? "Warnings: none"
                : "Warnings:" + Environment.NewLine + string.Join(Environment.NewLine, result.Warnings.Select(warning => "- " + warning)));
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        rawInputTextBox.Clear();
        reportOutputTextBox.Clear();
    }

    private bool TryShowParseErrors(ParseResult parseResult)
    {
        if (parseResult.IsSuccess)
        {
            return true;
        }

        reportOutputTextBox.Text = reportBuilder.BuildParseReport(parseResult, ReportLanguage.English);
        return false;
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "SurveyCalcKit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static string FormatPointForInput(PointRecord point)
    {
        return point.H.HasValue
            ? $"{point.Name} {FormatNumber(point.X)} {FormatNumber(point.Y)} {FormatNumber(point.H.Value)}"
            : $"{point.Name} {FormatNumber(point.X)} {FormatNumber(point.Y)}";
    }

    private static CoordinateForwardResult CreateEmptyForwardResult()
    {
        return new CoordinateForwardResult(string.Empty, 0, 0, 0, 0, 0, 0, string.Empty, 0, 0, new List<string>());
    }

    private static ChainageOffsetResult CreateEmptyChainageOffsetResult()
    {
        return new ChainageOffsetResult(string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, "Undefined", false, 0, 0, new List<string>());
    }

    private static CoordinateInverseResult CreateEmptyInverseResult()
    {
        return new CoordinateInverseResult(string.Empty, string.Empty, 0, 0, 0, 0, null, null, new List<string>());
    }

    private static TraverseQualityResult CreateEmptyTraverseQualityResult()
    {
        return new TraverseQualityResult(0, 0, 0, 0, 0, 0, 0, null, null, null, null, "NotEvaluated", new List<string>(), new List<TraverseQualitySegmentRow>());
    }

    private static CircularCurveResult CreateEmptyCircularCurveResult()
    {
        return new CircularCurveResult(string.Empty, 0, 0, 0, string.Empty, 0, 0, 0, 0, 0, 0, new List<string>());
    }

    private static VerticalCurveResult CreateEmptyVerticalCurveResult()
    {
        return new VerticalCurveResult(string.Empty, 0, 0, 0, 0, 0, 0, "NotEvaluated", 0, 0, 0, 0, new List<VerticalCurvePointResult>(), new List<string>());
    }

    private static StakeoutBatchResult CreateEmptyStakeoutBatchResult()
    {
        return new StakeoutBatchResult(string.Empty, 0, 0, 0, 0, new List<StakeoutPointResult>(), new List<string>());
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
