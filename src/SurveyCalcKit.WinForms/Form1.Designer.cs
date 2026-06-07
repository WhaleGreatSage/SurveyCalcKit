namespace SurveyCalcKit.WinForms;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null!;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        rawInputTextBox = new TextBox();
        reportOutputTextBox = new TextBox();
        importButton = new Button();
        importExcelButton = new Button();
        calculateTraverseButton = new Button();
        calculateElevationButton = new Button();
        calculateLevelingButton = new Button();
        calculateClosureButton = new Button();
        calculateForwardButton = new Button();
        calculateOffsetButton = new Button();
        exportExcelButton = new Button();
        exportReportButton = new Button();
        clearButton = new Button();
        openFileDialog = new OpenFileDialog();
        saveFileDialog = new SaveFileDialog();
        rootLayout = new TableLayoutPanel();
        textLayout = new TableLayoutPanel();
        buttonPanel = new FlowLayoutPanel();
        rootLayout.SuspendLayout();
        textLayout.SuspendLayout();
        buttonPanel.SuspendLayout();
        SuspendLayout();
        //
        // rawInputTextBox
        //
        rawInputTextBox.AcceptsTab = true;
        rawInputTextBox.Dock = DockStyle.Fill;
        rawInputTextBox.Font = new Font("Consolas", 10F);
        rawInputTextBox.Location = new Point(3, 3);
        rawInputTextBox.Multiline = true;
        rawInputTextBox.Name = "rawInputTextBox";
        rawInputTextBox.ScrollBars = ScrollBars.Both;
        rawInputTextBox.Size = new Size(531, 554);
        rawInputTextBox.TabIndex = 0;
        rawInputTextBox.WordWrap = false;
        //
        // reportOutputTextBox
        //
        reportOutputTextBox.Dock = DockStyle.Fill;
        reportOutputTextBox.Font = new Font("Consolas", 10F);
        reportOutputTextBox.Location = new Point(540, 3);
        reportOutputTextBox.Multiline = true;
        reportOutputTextBox.Name = "reportOutputTextBox";
        reportOutputTextBox.ReadOnly = true;
        reportOutputTextBox.ScrollBars = ScrollBars.Both;
        reportOutputTextBox.Size = new Size(532, 554);
        reportOutputTextBox.TabIndex = 1;
        reportOutputTextBox.WordWrap = false;
        //
        // importButton
        //
        importButton.AutoSize = true;
        importButton.Location = new Point(3, 3);
        importButton.Name = "importButton";
        importButton.Size = new Size(86, 30);
        importButton.TabIndex = 0;
        importButton.Text = "Import";
        importButton.UseVisualStyleBackColor = true;
        importButton.Click += ImportButton_Click;
        //
        // importExcelButton
        //
        importExcelButton.AutoSize = true;
        importExcelButton.Location = new Point(95, 3);
        importExcelButton.Name = "importExcelButton";
        importExcelButton.Size = new Size(91, 30);
        importExcelButton.TabIndex = 1;
        importExcelButton.Text = "导入 Excel";
        importExcelButton.UseVisualStyleBackColor = true;
        importExcelButton.Click += ImportExcelButton_Click;
        //
        // calculateTraverseButton
        //
        calculateTraverseButton.AutoSize = true;
        calculateTraverseButton.Location = new Point(192, 3);
        calculateTraverseButton.Name = "calculateTraverseButton";
        calculateTraverseButton.Size = new Size(134, 30);
        calculateTraverseButton.TabIndex = 2;
        calculateTraverseButton.Text = "Calculate Traverse";
        calculateTraverseButton.UseVisualStyleBackColor = true;
        calculateTraverseButton.Click += CalculateTraverseButton_Click;
        //
        // calculateElevationButton
        //
        calculateElevationButton.AutoSize = true;
        calculateElevationButton.Location = new Point(332, 3);
        calculateElevationButton.Name = "calculateElevationButton";
        calculateElevationButton.Size = new Size(138, 30);
        calculateElevationButton.TabIndex = 3;
        calculateElevationButton.Text = "Calculate Elevation";
        calculateElevationButton.UseVisualStyleBackColor = true;
        calculateElevationButton.Click += CalculateElevationButton_Click;
        //
        // calculateLevelingButton
        //
        calculateLevelingButton.AutoSize = true;
        calculateLevelingButton.Location = new Point(476, 3);
        calculateLevelingButton.Name = "calculateLevelingButton";
        calculateLevelingButton.Size = new Size(124, 30);
        calculateLevelingButton.TabIndex = 4;
        calculateLevelingButton.Text = "Calculate Leveling";
        calculateLevelingButton.UseVisualStyleBackColor = true;
        calculateLevelingButton.Click += CalculateLevelingButton_Click;
        //
        // calculateClosureButton
        //
        calculateClosureButton.AutoSize = true;
        calculateClosureButton.Location = new Point(606, 3);
        calculateClosureButton.Name = "calculateClosureButton";
        calculateClosureButton.Size = new Size(125, 30);
        calculateClosureButton.TabIndex = 5;
        calculateClosureButton.Text = "Calculate Closure";
        calculateClosureButton.UseVisualStyleBackColor = true;
        calculateClosureButton.Click += CalculateClosureButton_Click;
        //
        // calculateForwardButton
        //
        calculateForwardButton.AutoSize = true;
        calculateForwardButton.Location = new Point(737, 3);
        calculateForwardButton.Name = "calculateForwardButton";
        calculateForwardButton.Size = new Size(119, 30);
        calculateForwardButton.TabIndex = 6;
        calculateForwardButton.Text = "Calculate Forward";
        calculateForwardButton.UseVisualStyleBackColor = true;
        calculateForwardButton.Click += CalculateForwardButton_Click;
        //
        // calculateOffsetButton
        //
        calculateOffsetButton.AutoSize = true;
        calculateOffsetButton.Location = new Point(862, 3);
        calculateOffsetButton.Name = "calculateOffsetButton";
        calculateOffsetButton.Size = new Size(111, 30);
        calculateOffsetButton.TabIndex = 7;
        calculateOffsetButton.Text = "Calculate Offset";
        calculateOffsetButton.UseVisualStyleBackColor = true;
        calculateOffsetButton.Click += CalculateOffsetButton_Click;
        //
        // exportExcelButton
        //
        exportExcelButton.AutoSize = true;
        exportExcelButton.Location = new Point(979, 3);
        exportExcelButton.Name = "exportExcelButton";
        exportExcelButton.Size = new Size(91, 30);
        exportExcelButton.TabIndex = 8;
        exportExcelButton.Text = "导出 Excel";
        exportExcelButton.UseVisualStyleBackColor = true;
        exportExcelButton.Click += ExportExcelButton_Click;
        //
        // exportReportButton
        //
        exportReportButton.AutoSize = true;
        exportReportButton.Location = new Point(3, 39);
        exportReportButton.Name = "exportReportButton";
        exportReportButton.Size = new Size(105, 30);
        exportReportButton.TabIndex = 9;
        exportReportButton.Text = "Export Report";
        exportReportButton.UseVisualStyleBackColor = true;
        exportReportButton.Click += ExportReportButton_Click;
        //
        // clearButton
        //
        clearButton.AutoSize = true;
        clearButton.Location = new Point(114, 39);
        clearButton.Name = "clearButton";
        clearButton.Size = new Size(75, 30);
        clearButton.TabIndex = 10;
        clearButton.Text = "Clear";
        clearButton.UseVisualStyleBackColor = true;
        clearButton.Click += ClearButton_Click;
        //
        // openFileDialog
        //
        openFileDialog.Filter = "Survey data (*.txt;*.dat;*.csv)|*.txt;*.dat;*.csv|All files (*.*)|*.*";
        openFileDialog.Title = "Import survey data";
        //
        // saveFileDialog
        //
        saveFileDialog.DefaultExt = "txt";
        saveFileDialog.Filter = "Text report (*.txt)|*.txt|All files (*.*)|*.*";
        saveFileDialog.Title = "Export calculation report";
        //
        // rootLayout
        //
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(textLayout, 0, 0);
        rootLayout.Controls.Add(buttonPanel, 0, 1);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Location = new Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.RowCount = 2;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
        rootLayout.Size = new Size(1081, 652);
        rootLayout.TabIndex = 0;
        //
        // textLayout
        //
        textLayout.ColumnCount = 2;
        textLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        textLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        textLayout.Controls.Add(rawInputTextBox, 0, 0);
        textLayout.Controls.Add(reportOutputTextBox, 1, 0);
        textLayout.Dock = DockStyle.Fill;
        textLayout.Location = new Point(3, 3);
        textLayout.Name = "textLayout";
        textLayout.RowCount = 1;
        textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        textLayout.Size = new Size(1075, 560);
        textLayout.TabIndex = 0;
        //
        // buttonPanel
        //
        buttonPanel.Controls.Add(importButton);
        buttonPanel.Controls.Add(importExcelButton);
        buttonPanel.Controls.Add(calculateTraverseButton);
        buttonPanel.Controls.Add(calculateElevationButton);
        buttonPanel.Controls.Add(calculateLevelingButton);
        buttonPanel.Controls.Add(calculateClosureButton);
        buttonPanel.Controls.Add(calculateForwardButton);
        buttonPanel.Controls.Add(calculateOffsetButton);
        buttonPanel.Controls.Add(exportExcelButton);
        buttonPanel.Controls.Add(exportReportButton);
        buttonPanel.Controls.Add(clearButton);
        buttonPanel.Dock = DockStyle.Fill;
        buttonPanel.Location = new Point(3, 569);
        buttonPanel.Name = "buttonPanel";
        buttonPanel.Padding = new Padding(0, 3, 0, 0);
        buttonPanel.Size = new Size(1075, 80);
        buttonPanel.TabIndex = 1;
        //
        // Form1
        //
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1081, 652);
        Controls.Add(rootLayout);
        MinimumSize = new Size(1000, 560);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SurveyCalcKit";
        rootLayout.ResumeLayout(false);
        textLayout.ResumeLayout(false);
        textLayout.PerformLayout();
        buttonPanel.ResumeLayout(false);
        buttonPanel.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TextBox rawInputTextBox;
    private TextBox reportOutputTextBox;
    private Button importButton;
    private Button importExcelButton;
    private Button calculateTraverseButton;
    private Button calculateElevationButton;
    private Button calculateLevelingButton;
    private Button calculateClosureButton;
    private Button calculateForwardButton;
    private Button calculateOffsetButton;
    private Button exportExcelButton;
    private Button exportReportButton;
    private Button clearButton;
    private OpenFileDialog openFileDialog;
    private SaveFileDialog saveFileDialog;
    private TableLayoutPanel rootLayout;
    private TableLayoutPanel textLayout;
    private FlowLayoutPanel buttonPanel;
}
