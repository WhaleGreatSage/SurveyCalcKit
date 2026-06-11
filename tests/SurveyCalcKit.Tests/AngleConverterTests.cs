using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class AngleConverterTests
{
    [Fact]
    public void Convert_DecimalDegreesToDms()
    {
        var converter = new AngleConverter();

        var result = converter.Convert(new AngleConversionInput(53.130102, null, null));

        Assert.Equal(53.130102, result.DecimalDegrees, 6);
        Assert.Equal("53°07'48.37\"", result.DmsText);
    }

    [Theory]
    [InlineData("53°07'48.37\"")]
    [InlineData("53 7 48.37")]
    [InlineData("53:7:48.37")]
    public void Convert_DmsToDecimalDegrees(string dms)
    {
        var converter = new AngleConverter();

        var result = converter.Convert(new AngleConversionInput(null, dms, null));

        Assert.Equal(53 + 7 / 60.0 + 48.37 / 3600.0, result.DecimalDegrees, 6);
    }

    [Fact]
    public void Convert_DecimalDegreesToRadians()
    {
        var converter = new AngleConverter();

        var result = converter.Convert(new AngleConversionInput(180, null, null));

        Assert.Equal(Math.PI, result.Radians, 6);
    }

    [Fact]
    public void Convert_RadiansToDecimalDegrees()
    {
        var converter = new AngleConverter();

        var result = converter.Convert(new AngleConversionInput(null, null, Math.PI / 2));

        Assert.Equal(90, result.DecimalDegrees, 6);
    }

    [Fact]
    public void Convert_HandlesNegativeAngles()
    {
        var converter = new AngleConverter();

        var result = converter.Convert(new AngleConversionInput(-12.5, null, null));

        Assert.Equal(-12.5, result.DecimalDegrees, 6);
        Assert.Equal("-12°30'00.00\"", result.DmsText);
    }

    [Fact]
    public void Convert_ReturnsWarningForInvalidDmsText()
    {
        var converter = new AngleConverter();

        var result = converter.Convert(new AngleConversionInput(null, "bad angle", null));

        Assert.Contains(result.Warnings, warning => warning.Contains("invalid", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Convert_NormalizesAzimuthWhenRequested()
    {
        var converter = new AngleConverter();

        var result = converter.Convert(new AngleConversionInput(-10, null, null, NormalizeAzimuth: true));

        Assert.Equal(350, result.DecimalDegrees, 6);
    }
}
