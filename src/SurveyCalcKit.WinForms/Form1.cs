using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.WinForms;

public partial class Form1 : Form
{
    private readonly ParseService parseService = new();
    private readonly TraverseCalculator traverseCalculator = new();
    private readonly ClosedTraverseCalculator closedTraverseCalculator = new();
    private readonly LevelingRouteCalculator levelingRouteCalculator = new();
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
}
