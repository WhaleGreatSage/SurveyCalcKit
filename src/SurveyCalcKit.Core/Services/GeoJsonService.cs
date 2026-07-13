using System.Globalization;
using System.Text;
using System.Text.Json;
using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class GeoJsonService
{
    public GeoJsonImportResult Import(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var points = new List<PointRecord>();
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var warnings = new List<string>();
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !TryGetString(root, "type", out var type))
            {
                warnings.Add("GeoJSON root must be an object with a type property.");
                return new GeoJsonImportResult(string.Empty, points, metadata, warnings);
            }

            if (string.Equals(type, "FeatureCollection", StringComparison.OrdinalIgnoreCase))
            {
                if (!root.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
                {
                    warnings.Add("FeatureCollection requires a features array.");
                    return new GeoJsonImportResult(type, points, metadata, warnings);
                }

                var featureIndex = 0;
                foreach (var feature in features.EnumerateArray())
                {
                    featureIndex++;
                    ImportFeature(feature, featureIndex, points, metadata, warnings);
                }

                return new GeoJsonImportResult(type, points, metadata, warnings);
            }

            ImportGeometry(root, type, "Geometry", points, warnings);
            return new GeoJsonImportResult(type, points, metadata, warnings);
        }
        catch (JsonException exception)
        {
            warnings.Add($"Invalid GeoJSON: {exception.Message}");
            return new GeoJsonImportResult(string.Empty, points, metadata, warnings);
        }
    }

    public GeoJsonExportResult Export(
        IEnumerable<PointRecord> sourcePoints,
        string outputPath,
        GeoJsonExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(sourcePoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(options);

        var points = sourcePoints.ToList();
        var warnings = new List<string>();
        var geometryType = NormalizeGeometryType(options.GeometryType);
        if (geometryType is null)
        {
            warnings.Add($"Unsupported GeoJSON export geometry '{options.GeometryType}'.");
            return new GeoJsonExportResult(outputPath, options.GeometryType, 0, warnings);
        }

        if (points.Count == 0)
        {
            warnings.Add("At least one point is required for GeoJSON export.");
            return new GeoJsonExportResult(outputPath, geometryType, 0, warnings);
        }

        if (geometryType == "LineString" && points.Count < 2)
        {
            warnings.Add("LineString export requires at least two points.");
            return new GeoJsonExportResult(outputPath, geometryType, 0, warnings);
        }

        if (geometryType == "Polygon" && points.Count < 3)
        {
            warnings.Add("Polygon export requires at least three points.");
            return new GeoJsonExportResult(outputPath, geometryType, 0, warnings);
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var coordinateCount = geometryType == "Point"
            ? points.Count
            : geometryType == "Polygon" && !CoordinatesMatch(points[0], points[^1])
                ? points.Count + 1
                : points.Count;
        using var stream = File.Create(outputPath);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("type", "FeatureCollection");
        writer.WritePropertyName("features");
        writer.WriteStartArray();
        if (geometryType == "Point")
        {
            foreach (var point in points)
            {
                WriteFeatureStart(writer, point.Name, options.Properties);
                writer.WritePropertyName("geometry");
                writer.WriteStartObject();
                writer.WriteString("type", "Point");
                writer.WritePropertyName("coordinates");
                WriteCoordinate(writer, point, options.IncludeElevation);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
        }
        else
        {
            WriteFeatureStart(writer, options.FeatureName, options.Properties);
            writer.WritePropertyName("geometry");
            writer.WriteStartObject();
            writer.WriteString("type", geometryType);
            writer.WritePropertyName("coordinates");
            if (geometryType == "LineString")
            {
                WriteCoordinateArray(writer, points, options.IncludeElevation);
            }
            else
            {
                writer.WriteStartArray();
                WriteCoordinateArray(writer, points, options.IncludeElevation, closeRing: true);
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
        return new GeoJsonExportResult(outputPath, geometryType, coordinateCount, warnings);
    }

    private static void ImportFeature(
        JsonElement feature,
        int featureIndex,
        List<PointRecord> points,
        Dictionary<string, string> metadata,
        List<string> warnings)
    {
        if (feature.ValueKind != JsonValueKind.Object || !feature.TryGetProperty("geometry", out var geometry))
        {
            warnings.Add($"Feature {featureIndex} is missing geometry.");
            return;
        }

        var featureName = ReadFeatureName(feature, featureIndex);
        if (feature.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in properties.EnumerateObject())
            {
                if (!metadata.ContainsKey(property.Name))
                {
                    metadata[property.Name] = property.Value.ToString();
                }
            }
        }

        if (!TryGetString(geometry, "type", out var geometryType))
        {
            warnings.Add($"Feature {featureIndex} has geometry without a type.");
            return;
        }

        ImportGeometry(geometry, geometryType, featureName, points, warnings);
    }

    private static void ImportGeometry(
        JsonElement geometry,
        string geometryType,
        string namePrefix,
        List<PointRecord> points,
        List<string> warnings)
    {
        if (!geometry.TryGetProperty("coordinates", out var coordinates))
        {
            warnings.Add($"{geometryType} geometry is missing coordinates.");
            return;
        }

        switch (geometryType.ToUpperInvariant())
        {
            case "POINT":
                if (TryReadCoordinate(coordinates, out var point))
                {
                    points.Add(point with { Name = namePrefix });
                }
                else
                {
                    warnings.Add($"Point {namePrefix} has invalid coordinates.");
                }

                break;
            case "LINESTRING":
                AddCoordinateSequence(coordinates, namePrefix, points, warnings, removeClosedEndpoint: false);
                break;
            case "POLYGON":
                if (coordinates.ValueKind != JsonValueKind.Array || coordinates.GetArrayLength() == 0)
                {
                    warnings.Add($"Polygon {namePrefix} has no exterior ring.");
                    break;
                }

                AddCoordinateSequence(coordinates[0], namePrefix, points, warnings, removeClosedEndpoint: true);
                break;
            default:
                warnings.Add($"Unsupported GeoJSON geometry type '{geometryType}'.");
                break;
        }
    }

    private static void AddCoordinateSequence(
        JsonElement coordinates,
        string namePrefix,
        List<PointRecord> points,
        List<string> warnings,
        bool removeClosedEndpoint)
    {
        if (coordinates.ValueKind != JsonValueKind.Array)
        {
            warnings.Add($"{namePrefix} coordinates must be an array.");
            return;
        }

        var sequence = new List<PointRecord>();
        foreach (var coordinate in coordinates.EnumerateArray())
        {
            if (!TryReadCoordinate(coordinate, out var point))
            {
                warnings.Add($"{namePrefix} contains an invalid coordinate.");
                continue;
            }

            sequence.Add(point);
        }

        if (removeClosedEndpoint && sequence.Count > 1 && CoordinatesMatch(sequence[0], sequence[^1]))
        {
            sequence.RemoveAt(sequence.Count - 1);
        }

        for (var index = 0; index < sequence.Count; index++)
        {
            points.Add(sequence[index] with { Name = $"{namePrefix}-{index + 1}" });
        }
    }

    private static bool TryReadCoordinate(JsonElement coordinate, out PointRecord point)
    {
        point = new PointRecord(string.Empty, 0, 0);
        if (coordinate.ValueKind != JsonValueKind.Array || coordinate.GetArrayLength() < 2 ||
            !coordinate[0].TryGetDouble(out var x) || !coordinate[1].TryGetDouble(out var y) ||
            !double.IsFinite(x) || !double.IsFinite(y))
        {
            return false;
        }

        double? h = null;
        if (coordinate.GetArrayLength() > 2 && coordinate[2].TryGetDouble(out var elevation) && double.IsFinite(elevation))
        {
            h = elevation;
        }

        point = new PointRecord(string.Empty, x, y, h);
        return true;
    }

    private static void WriteFeatureStart(Utf8JsonWriter writer, string featureName, IReadOnlyDictionary<string, string> properties)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "Feature");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        if (!string.IsNullOrWhiteSpace(featureName))
        {
            writer.WriteString("name", featureName);
        }

        foreach (var property in properties)
        {
            writer.WriteString(property.Key, property.Value);
        }

        writer.WriteEndObject();
    }

    private static void WriteCoordinateArray(Utf8JsonWriter writer, IReadOnlyList<PointRecord> points, bool includeElevation, bool closeRing = false)
    {
        writer.WriteStartArray();
        foreach (var point in points)
        {
            WriteCoordinate(writer, point, includeElevation);
        }

        if (closeRing && !CoordinatesMatch(points[0], points[^1]))
        {
            WriteCoordinate(writer, points[0], includeElevation);
        }

        writer.WriteEndArray();
    }

    private static void WriteCoordinate(Utf8JsonWriter writer, PointRecord point, bool includeElevation)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(point.X);
        writer.WriteNumberValue(point.Y);
        if (includeElevation && point.H.HasValue)
        {
            writer.WriteNumberValue(point.H.Value);
        }

        writer.WriteEndArray();
    }

    private static string? NormalizeGeometryType(string geometryType)
    {
        return geometryType.Trim().ToUpperInvariant() switch
        {
            "POINT" or "POINTS" => "Point",
            "LINE" or "LINESTRING" => "LineString",
            "POLYGON" => "Polygon",
            _ => null
        };
    }

    private static string ReadFeatureName(JsonElement feature, int featureIndex)
    {
        if (feature.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
        {
            if (TryGetString(properties, "name", out var name) && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (TryGetString(properties, "id", out var id) && !string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
        }

        if (TryGetString(feature, "id", out var featureId) && !string.IsNullOrWhiteSpace(featureId))
        {
            return featureId;
        }

        return $"Feature{featureIndex}";
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String &&
               (value = property.GetString() ?? string.Empty) is not null;
    }

    private static bool CoordinatesMatch(PointRecord first, PointRecord second) =>
        Math.Abs(first.X - second.X) <= 1e-12 && Math.Abs(first.Y - second.Y) <= 1e-12;
}
