using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class HorizontalAlignmentBuilder
{
    public HorizontalAlignmentResult Build(HorizontalAlignmentInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var elements = new List<IAlignmentElement>();
        var summaries = new List<AlignmentElementSummary>();
        var current = new AlignmentState(
            input.StartChainage,
            input.StartX,
            input.StartY,
            AlignmentMath.NormalizeAzimuth(input.StartAzimuthDegrees),
            0,
            "START",
            "START");

        foreach (var definition in input.Elements)
        {
            var element = CreateElement(definition, current, warnings);
            if (element is null)
            {
                continue;
            }

            elements.Add(element);
            summaries.Add(new AlignmentElementSummary(
                element.ElementType,
                element.Name,
                element.StartChainage,
                element.EndState.Chainage,
                element.Length,
                element.StartState.X,
                element.StartState.Y,
                element.EndState.X,
                element.EndState.Y,
                element.StartState.AzimuthDegrees,
                element.EndState.AzimuthDegrees,
                element.StartState.Curvature,
                element.EndState.Curvature));
            current = element.EndState;
        }

        if (elements.Count == 0)
        {
            warnings.Add("Alignment requires at least one valid element.");
        }

        var alignment = new HorizontalAlignment(input.AlignmentName, input.StartChainage, elements);
        return new HorizontalAlignmentResult(
            input.AlignmentName,
            input.StartChainage,
            current.Chainage,
            current.Chainage - input.StartChainage,
            summaries,
            warnings,
            alignment);
    }

    private static IAlignmentElement? CreateElement(
        AlignmentElementDefinition definition,
        AlignmentState current,
        List<string> warnings)
    {
        var type = definition.Type.ToUpperInvariant();
        switch (type)
        {
            case "TANGENT":
                if (!TryGetPositiveLength(definition.Length, definition.Name, warnings, out var tangentLength))
                {
                    return null;
                }

                WarnForCurvatureMismatch(current.Curvature, 0, definition.Name, warnings);
                return new TangentAlignmentElement(definition.Name, current, tangentLength);

            case "CLOTHOID":
                if (!TryGetPositiveLength(definition.Length, definition.Name, warnings, out var spiralLength) ||
                    !TryGetPositiveRadius(definition.Radius, definition.Name, warnings, out var spiralRadius) ||
                    !AlignmentMath.TryGetDirectionSign(definition.Direction, out var spiralSign))
                {
                    if (!AlignmentMath.TryGetDirectionSign(definition.Direction, out _))
                    {
                        warnings.Add($"Clothoid {definition.Name} requires DIRECTION LEFT or RIGHT.");
                    }

                    return null;
                }

                var targetCurvature = spiralSign / spiralRadius;
                var startCurvature = definition.Reverse ? targetCurvature : 0;
                var endCurvature = definition.Reverse ? 0 : targetCurvature;
                WarnForCurvatureMismatch(current.Curvature, startCurvature, definition.Name, warnings);
                return new ClothoidAlignmentElement(definition.Name, current, spiralLength, startCurvature, endCurvature);

            case "ARC":
                if (!TryGetPositiveRadius(definition.Radius, definition.Name, warnings, out var arcRadius) ||
                    !AlignmentMath.TryGetDirectionSign(definition.Direction, out var arcSign))
                {
                    if (!AlignmentMath.TryGetDirectionSign(definition.Direction, out _))
                    {
                        warnings.Add($"Arc {definition.Name} requires DIRECTION LEFT or RIGHT.");
                    }

                    return null;
                }

                var arcLength = definition.Length;
                if (!arcLength.HasValue && definition.AngleDegrees.HasValue)
                {
                    arcLength = arcRadius * Math.Abs(AlignmentMath.ToRadians(definition.AngleDegrees.Value));
                }

                if (!TryGetPositiveLength(arcLength, definition.Name, warnings, out var resolvedArcLength))
                {
                    return null;
                }

                var arcCurvature = arcSign / arcRadius;
                WarnForCurvatureMismatch(current.Curvature, arcCurvature, definition.Name, warnings);
                return new CircularArcAlignmentElement(definition.Name, current, resolvedArcLength, arcCurvature);

            default:
                warnings.Add($"Unsupported alignment element type '{definition.Type}' for {definition.Name}.");
                return null;
        }
    }

    private static bool TryGetPositiveLength(double? value, string name, List<string> warnings, out double length)
    {
        length = value ?? 0;
        if (!double.IsFinite(length) || length <= 0)
        {
            warnings.Add($"Element {name} requires a positive length.");
            return false;
        }

        return true;
    }

    private static bool TryGetPositiveRadius(double? value, string name, List<string> warnings, out double radius)
    {
        radius = value ?? 0;
        if (!double.IsFinite(radius) || radius <= 0)
        {
            warnings.Add($"Element {name} requires a positive radius.");
            return false;
        }

        return true;
    }

    private static void WarnForCurvatureMismatch(double actual, double expected, string name, List<string> warnings)
    {
        if (Math.Abs(actual - expected) > 1e-8)
        {
            warnings.Add($"Curvature discontinuity before element {name}: expected {expected:0.########}, found {actual:0.########}.");
        }
    }
}
