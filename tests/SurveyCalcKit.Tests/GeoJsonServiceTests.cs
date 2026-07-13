using System.Text.Json;
using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class GeoJsonServiceTests
{
    [Fact]
    public void Import_ParsesPointLineAndPolygonFeatureCollections()
    {
        var service = new GeoJsonService();
        var pointResult = service.Import("""{"type":"FeatureCollection","features":[{"type":"Feature","properties":{"name":"P1"},"geometry":{"type":"Point","coordinates":[1,2,3]}}]}""");
        var lineResult = service.Import("""{"type":"FeatureCollection","features":[{"type":"Feature","properties":{"name":"L"},"geometry":{"type":"LineString","coordinates":[[0,0],[10,0]]}}]}""");
        var polygonResult = service.Import("""{"type":"FeatureCollection","features":[{"type":"Feature","properties":{"name":"A"},"geometry":{"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,0]]]}}]}""");

        Assert.Equal(3, pointResult.Points[0].H);
        Assert.Equal(2, lineResult.Points.Count);
        Assert.Equal(3, polygonResult.Points.Count);
    }

    [Fact]
    public void Export_WritesPointsLineAndClosedPolygonThatCanBeRead()
    {
        var points = new[] { new PointRecord("P1", 0, 0), new PointRecord("P2", 10, 0), new PointRecord("P3", 10, 10) };
        var directory = Path.Combine(Path.GetTempPath(), $"surveycalckit-geojson-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var service = new GeoJsonService();
            var pointPath = Path.Combine(directory, "points.geojson");
            var linePath = Path.Combine(directory, "line.geojson");
            var polygonPath = Path.Combine(directory, "polygon.geojson");

            service.Export(points, pointPath, new GeoJsonExportOptions("Point", "测试点", false, new Dictionary<string, string> { ["description"] = "中文属性" }));
            service.Export(points, linePath, new GeoJsonExportOptions("LineString", "Line", false, new Dictionary<string, string>()));
            service.Export(points, polygonPath, new GeoJsonExportOptions("Polygon", "Area", false, new Dictionary<string, string>()));

            using var pointDocument = JsonDocument.Parse(File.ReadAllText(pointPath));
            using var lineDocument = JsonDocument.Parse(File.ReadAllText(linePath));
            using var polygonDocument = JsonDocument.Parse(File.ReadAllText(polygonPath));
            Assert.Equal("FeatureCollection", pointDocument.RootElement.GetProperty("type").GetString());
            Assert.Equal("中文属性", pointDocument.RootElement.GetProperty("features")[0].GetProperty("properties").GetProperty("description").GetString());
            Assert.Equal("LineString", lineDocument.RootElement.GetProperty("features")[0].GetProperty("geometry").GetProperty("type").GetString());
            var ring = polygonDocument.RootElement.GetProperty("features")[0].GetProperty("geometry").GetProperty("coordinates")[0];
            Assert.Equal(ring[0][0].GetDouble(), ring[ring.GetArrayLength() - 1][0].GetDouble());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_ReturnsWarningsForInvalidAndUnsupportedGeometry()
    {
        var service = new GeoJsonService();

        var invalid = service.Import("not-json");
        var unsupported = service.Import("""{"type":"FeatureCollection","features":[{"type":"Feature","geometry":{"type":"MultiPoint","coordinates":[]}}]}""");

        Assert.NotEmpty(invalid.Warnings);
        Assert.Contains(unsupported.Warnings, warning => warning.Contains("Unsupported", StringComparison.OrdinalIgnoreCase));
    }
}
