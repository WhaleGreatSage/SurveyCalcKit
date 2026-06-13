using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class StakeoutBatchCalculatorTests
{
    [Fact]
    public void Calculate_ComputesBasicStakeoutPoint()
    {
        var result = Calculate(azimuth: 0, chainage: 20, offset: 0);

        Assert.Single(result.Points);
        Assert.Equal(1020, result.Points[0].X, 6);
        Assert.Equal(1000, result.Points[0].Y, 6);
    }

    [Fact]
    public void Calculate_SupportsZeroOffset()
    {
        var result = Calculate(azimuth: 35, chainage: 20, offset: 0);

        Assert.Equal("OnLine", result.Points[0].Side);
        Assert.Equal(0, result.Points[0].Offset, 6);
    }

    [Fact]
    public void Calculate_PositiveOffsetIsLeftSide()
    {
        var result = Calculate(azimuth: 0, chainage: 40, offset: 5);

        Assert.Equal("Left", result.Points[0].Side);
        Assert.Equal(1040, result.Points[0].X, 6);
        Assert.Equal(1005, result.Points[0].Y, 6);
    }

    [Fact]
    public void Calculate_NegativeOffsetIsRightSide()
    {
        var result = Calculate(azimuth: 0, chainage: 60, offset: -3);

        Assert.Equal("Right", result.Points[0].Side);
        Assert.Equal(1060, result.Points[0].X, 6);
        Assert.Equal(997, result.Points[0].Y, 6);
    }

    [Fact]
    public void Calculate_ComputesMultiplePoints()
    {
        var calculator = new StakeoutBatchCalculator();
        var input = CreateInput(new List<StakeoutRecord>
        {
            new("K0+020", 20, 0),
            new("K0+040_L5", 40, 5),
            new("K0+060_R3", 60, -3)
        });

        var result = calculator.Calculate(input);

        Assert.Equal(3, result.Points.Count);
        Assert.Equal("K0+060_R3", result.Points[2].PointName);
    }

    [Fact]
    public void Calculate_AddsWarningForDuplicatePointName()
    {
        var calculator = new StakeoutBatchCalculator();
        var input = CreateInput(new List<StakeoutRecord>
        {
            new("P1", 20, 0),
            new("P1", 40, 5)
        });

        var result = calculator.Calculate(input);

        Assert.Contains(result.Warnings, warning => warning.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseStakeoutBatch_ParsesValidFile()
    {
        var parser = new ParseService();

        var result = parser.ParseStakeoutBatch(
            """
            ORIGIN A 1000.000 1000.000
            AZIMUTH 35.0000
            START_CHAINAGE 0.000
            POINT K0+020 20.000 0.000
            POINT K0+040_L5 40.000 5.000
            POINT K0+060_R3 60.000 -3.000
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("A", result.Input!.OriginPointName);
        Assert.Equal(35, result.Input.BaselineAzimuthDegrees, 4);
        Assert.Equal(3, result.Input.Records.Count);
    }

    [Fact]
    public void ParseStakeoutBatch_ParsesCommaSeparatedFile()
    {
        var parser = new ParseService();

        var result = parser.ParseStakeoutBatch(
            """
            ORIGIN,A,1000.000,1000.000
            AZIMUTH,35.0000
            START_CHAINAGE,0.000
            POINT,K0+020,20.000,0.000
            POINT,K0+040_L5,40.000,5.000
            POINT,K0+060_R3,60.000,-3.000
            """);

        Assert.True(result.IsSuccess);
        Assert.Equal("K0+040_L5", result.Input!.Records[1].PointName);
        Assert.Equal(5, result.Input.Records[1].Offset, 3);
    }

    private static StakeoutBatchResult Calculate(double azimuth, double chainage, double offset)
    {
        var calculator = new StakeoutBatchCalculator();
        var input = CreateInput(new List<StakeoutRecord> { new("P1", chainage, offset) }, azimuth);
        return calculator.Calculate(input);
    }

    private static StakeoutBatchInput CreateInput(IReadOnlyList<StakeoutRecord> records, double azimuth = 0)
    {
        return new StakeoutBatchInput("A", 1000, 1000, azimuth, 0, records.ToList());
    }
}
