using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class ClothoidAlignmentElement : IAlignmentElement
{
    public ClothoidAlignmentElement(
        string name,
        AlignmentState startState,
        double length,
        double startCurvature,
        double endCurvature)
    {
        Name = name;
        StartState = startState;
        Length = length;
        StartChainage = startState.Chainage;
        StartCurvature = startCurvature;
        EndCurvature = endCurvature;
        EndState = GetStateAt(length);
    }

    public string Name { get; }
    public string ElementType => "CLOTHOID";
    public double StartChainage { get; }
    public double Length { get; }
    public AlignmentState StartState { get; }
    public AlignmentState EndState { get; }
    public double StartCurvature { get; }
    public double EndCurvature { get; }

    public AlignmentState GetStateAt(double localDistance)
    {
        var distance = Math.Clamp(localDistance, 0, Length);
        var startHeadingRadians = AlignmentMath.ToRadians(StartState.AzimuthDegrees);
        var curvatureRate = (EndCurvature - StartCurvature) / Length;
        var intervals = Math.Max(64, (int)Math.Ceiling(256 * distance / Length));
        var x = AlignmentMath.SimpsonIntegrate(
            u => Math.Cos(startHeadingRadians + StartCurvature * u + curvatureRate * u * u / 2.0),
            distance,
            intervals);
        var y = AlignmentMath.SimpsonIntegrate(
            u => Math.Sin(startHeadingRadians + StartCurvature * u + curvatureRate * u * u / 2.0),
            distance,
            intervals);
        var curvature = StartCurvature + curvatureRate * distance;
        var heading = startHeadingRadians + StartCurvature * distance + curvatureRate * distance * distance / 2.0;

        return new AlignmentState(
            StartChainage + distance,
            StartState.X + x,
            StartState.Y + y,
            AlignmentMath.NormalizeAzimuth(AlignmentMath.ToDegrees(heading)),
            curvature,
            ElementType,
            Name);
    }
}
