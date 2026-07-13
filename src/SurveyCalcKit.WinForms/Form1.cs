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
    private readonly ClothoidCalculator clothoidCalculator = new();
    private readonly HorizontalAlignmentBuilder horizontalAlignmentBuilder = new();
    private readonly AlignmentQueryService alignmentQueryService = new();
    private readonly CenterlineOffsetCalculator centerlineOffsetCalculator = new();
    private readonly CoordinateForwardCalculator coordinateForwardCalculator = new();
    private readonly CoordinateInverseCalculator coordinateInverseCalculator = new();
    private readonly ChainageOffsetCalculator chainageOffsetCalculator = new();
    private readonly StakeoutBatchCalculator stakeoutBatchCalculator = new();
    private readonly BatchSegmentTableCalculator batchSegmentTableCalculator = new();
    private readonly AngleConverter angleConverter = new();
    private readonly MarkdownReportExporter markdownReportExporter = new();
    private readonly DxfExporter dxfExporter = new();
    private readonly GeoJsonService geoJsonService = new();
    private readonly ExcelService excelService = new();
    private readonly ReportBuilder reportBuilder = new();
    private readonly ToolStripStatusLabel statusLabel = new();

    public Form1()
    {
        InitializeComponent();
        ConfigureToolTabs();
    }

    private void ConfigureToolTabs()
    {
        rootLayout.Controls.Remove(buttonPanel);
        rootLayout.RowCount = 3;
        rootLayout.RowStyles.Clear();
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 142F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

        var toolTabs = new TabControl { Dock = DockStyle.Fill };
        var basicTab = CreateToolTab("Basic Calculations", buttonPanel);
        var traverseTab = CreateToolTab("Traverse and Leveling");
        var routeTab = CreateToolTab("Route Alignment");
        var importExportTab = CreateToolTab("Import and Export");
        var reportsTab = CreateToolTab("Reports");

        MoveButtons(
            buttonPanel,
            calculateTraverseButton,
            calculateElevationButton,
            calculateForwardButton,
            calculateInverseButton,
            calculateOffsetButton,
            calculateSegmentsButton,
            convertAngleButton);
        MoveButtons(
            GetToolPanel(traverseTab),
            calculateClosureButton,
            evaluateQualityButton,
            calculateLevelingButton);
        MoveButtons(
            GetToolPanel(routeTab),
            calculateCurveButton,
            calculateVerticalCurveButton,
            calculateStakeoutButton,
            CreateToolButton("Calculate Clothoid", CalculateClothoidButton_Click),
            CreateToolButton("Load Alignment", LoadAlignmentButton_Click),
            CreateToolButton("Query Alignment", QueryAlignmentButton_Click),
            CreateToolButton("Centerline Offset", CenterlineOffsetButton_Click));
        MoveButtons(
            GetToolPanel(importExportTab),
            importButton,
            importExcelButton,
            CreateToolButton("Import GeoJSON", ImportGeoJsonButton_Click),
            exportExcelButton,
            exportDxfButton,
            CreateToolButton("Export GeoJSON", ExportGeoJsonButton_Click));
        MoveButtons(
            GetToolPanel(reportsTab),
            exportReportButton,
            exportMarkdownButton,
            clearButton);

        toolTabs.TabPages.Add(basicTab);
        toolTabs.TabPages.Add(traverseTab);
        toolTabs.TabPages.Add(routeTab);
        toolTabs.TabPages.Add(importExportTab);
        toolTabs.TabPages.Add(reportsTab);

        var statusStrip = new StatusStrip { Dock = DockStyle.Fill, SizingGrip = false };
        statusLabel.Text = "Ready. No data loaded.";
        statusStrip.Items.Add(statusLabel);
        rootLayout.Controls.Add(toolTabs, 0, 1);
        rootLayout.Controls.Add(statusStrip, 0, 2);
        MinimumSize = new Size(1080, 640);
    }

    private static TabPage CreateToolTab(string title, FlowLayoutPanel? existingPanel = null)
    {
        var panel = existingPanel ?? new FlowLayoutPanel();
        panel.Dock = DockStyle.Fill;
        panel.AutoScroll = true;
        panel.WrapContents = true;
        panel.Padding = new Padding(8);
        var tab = new TabPage(title);
        tab.Controls.Add(panel);
        return tab;
    }

    private static FlowLayoutPanel GetToolPanel(TabPage tab) => (FlowLayoutPanel)tab.Controls[0];

    private static Button CreateToolButton(string text, EventHandler clickHandler)
    {
        var button = new Button
        {
            AutoSize = true,
            Text = text,
            Margin = new Padding(4)
        };
        button.Click += clickHandler;
        return button;
    }

    private static void MoveButtons(FlowLayoutPanel target, params Button[] buttons)
    {
        foreach (var button in buttons)
        {
            target.Controls.Add(button);
        }
    }

    private void SetStatus(string dataType, string message)
    {
        statusLabel.Text = $"{dataType}: {message}";
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
            SetStatus(Path.GetExtension(openFileDialog.FileName).TrimStart('.').ToUpperInvariant(), "Raw input loaded.");
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
            SetStatus("Excel", "Import failed.");
            return;
        }

        rawInputTextBox.Text = string.Join(Environment.NewLine, result.Points.Select(FormatPointForInput));
        reportOutputTextBox.Text = $"Imported {result.Points.Count} point(s) from Excel.";
        SetStatus("Excel points", $"Imported {result.Points.Count} point(s).");
    }

    private void ImportGeoJsonButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "GeoJSON (*.geojson;*.json)|*.geojson;*.json|All files (*.*)|*.*",
            Title = "Import GeoJSON"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = geoJsonService.Import(File.ReadAllText(dialog.FileName));
        rawInputTextBox.Text = string.Join(Environment.NewLine, result.Points.Select(FormatPointForInput));
        reportOutputTextBox.Text = reportBuilder.BuildGeoJsonImportReport(result, ReportLanguage.English);
        SetStatus("GeoJSON", result.Points.Count > 0 ? $"Imported {result.Points.Count} point(s)." : "No supported coordinates were imported.");
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

    private void CalculateClothoidButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseClothoid(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildClothoidReport(parseResult, CreateEmptyClothoidResult(), ReportLanguage.English);
            SetStatus("Clothoid", "Input contains errors.");
            return;
        }

        var result = clothoidCalculator.Calculate(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildClothoidReport(parseResult, result, ReportLanguage.English);
        SetStatus("Clothoid", result.Warnings.Count == 0 ? "Calculation complete." : "Calculation completed with warnings.");
    }

    private void LoadAlignmentButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseHorizontalAlignment(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildHorizontalAlignmentReport(parseResult, CreateEmptyAlignmentResult(), ReportLanguage.English);
            SetStatus("Alignment", "Input contains errors.");
            return;
        }

        var result = horizontalAlignmentBuilder.Build(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildHorizontalAlignmentReport(parseResult, result, ReportLanguage.English);
        SetStatus("Alignment", result.Warnings.Count == 0 ? "Alignment loaded." : "Alignment loaded with continuity warnings.");
    }

    private void QueryAlignmentButton_Click(object? sender, EventArgs e)
    {
        var alignmentParseResult = parseService.ParseHorizontalAlignment(rawInputTextBox.Text);
        if (!alignmentParseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildHorizontalAlignmentReport(alignmentParseResult, CreateEmptyAlignmentResult(), ReportLanguage.English);
            SetStatus("Alignment query", "Alignment input contains errors.");
            return;
        }

        var alignmentResult = horizontalAlignmentBuilder.Build(alignmentParseResult.Input!);
        if (alignmentResult.Alignment is null)
        {
            reportOutputTextBox.Text = reportBuilder.BuildHorizontalAlignmentReport(alignmentParseResult, alignmentResult, ReportLanguage.English);
            SetStatus("Alignment query", "Alignment could not be built.");
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "Chainage list (*.txt;*.csv)|*.txt;*.csv|All files (*.*)|*.*",
            Title = "Select alignment chainage list"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var chainageParseResult = parseService.ParseChainages(File.ReadAllText(dialog.FileName));
        if (!chainageParseResult.IsSuccess)
        {
            reportOutputTextBox.Text = string.Join(Environment.NewLine, chainageParseResult.Errors.Select(error => error.Message));
            SetStatus("Alignment query", "Chainage list contains errors.");
            return;
        }

        var result = alignmentQueryService.Query(new AlignmentQueryInput(alignmentResult.Alignment, chainageParseResult.Chainages));
        reportOutputTextBox.Text = reportBuilder.BuildAlignmentQueryReport(result, ReportLanguage.English);
        SetStatus("Alignment query", result.Warnings.Count == 0 ? "Query complete." : "Query completed with warnings.");
    }

    private void CenterlineOffsetButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParseCenterlineOffset(rawInputTextBox.Text);
        if (!parseResult.IsSuccess)
        {
            reportOutputTextBox.Text = reportBuilder.BuildCenterlineOffsetReport(parseResult, CreateEmptyCenterlineOffsetResult(), ReportLanguage.English);
            SetStatus("Centerline offset", "Input contains errors.");
            return;
        }

        var result = centerlineOffsetCalculator.Calculate(parseResult.Input!);
        reportOutputTextBox.Text = reportBuilder.BuildCenterlineOffsetReport(parseResult, result, ReportLanguage.English);
        SetStatus("Centerline offset", result.Warnings.Count == 0 ? "Calculation complete." : "Calculation completed with warnings.");
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
        SetStatus("DXF", $"Exported {result.PointCount} point(s).");
    }

    private void ExportGeoJsonButton_Click(object? sender, EventArgs e)
    {
        var parseResult = parseService.ParsePoints(rawInputTextBox.Text);
        if (!TryShowParseErrors(parseResult))
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            DefaultExt = "geojson",
            Filter = "GeoJSON (*.geojson)|*.geojson|All files (*.*)|*.*",
            Title = "Export GeoJSON LineString"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = geoJsonService.Export(
            parseResult.Points,
            dialog.FileName,
            new GeoJsonExportOptions("LineString", "SurveyCalcKit line", true, new Dictionary<string, string>()));
        reportOutputTextBox.Text = reportBuilder.BuildGeoJsonExportReport(result, ReportLanguage.English);
        SetStatus("GeoJSON", result.Warnings.Count == 0 ? "LineString exported." : "Export completed with warnings.");
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        rawInputTextBox.Clear();
        reportOutputTextBox.Clear();
        SetStatus("Ready", "Input and report cleared.");
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

    private void ShowError(string message)
    {
        SetStatus("Error", message);
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

    private static ClothoidResult CreateEmptyClothoidResult()
    {
        return new ClothoidResult(string.Empty, 0, 0, 0, 0, 0, 0, 0, 0, string.Empty, new List<ClothoidPointResult>(), new List<string>());
    }

    private static HorizontalAlignmentResult CreateEmptyAlignmentResult()
    {
        return new HorizontalAlignmentResult(string.Empty, 0, 0, 0, new List<AlignmentElementSummary>(), new List<string>(), null);
    }

    private static CenterlineOffsetResult CreateEmptyCenterlineOffsetResult()
    {
        return new CenterlineOffsetResult(0, 0, new List<CenterlineOffsetPointResult>(), new List<string>());
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
