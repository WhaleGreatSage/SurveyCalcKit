using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class CircularArcAlignmentElement : IAlignmentElement
{
    public CircularArcAlignmentElement(string name, AlignmentState startState, double length, double signedCurvature)
    {
        Name = name;
        StartState = startState;
        Length = length;
        StartChainage = startState.Chainage;
        SignedCurvature = signedCurvature;
        EndState = GetStateAt(length);
    }

    public string Name { get; }
    public string ElementType => "ARC";
    public double StartChainage { get; }
    public double Length { get; }
    public AlignmentState StartState { get; }
    public AlignmentState EndState { get; }
    public double SignedCurvature { get; }

    public AlignmentState GetStateAt(double localDistance)
    {
        var distance = Math.Clamp(localDistance, 0, Length);
        var startHeading = AlignmentMath.ToRadians(StartState.AzimuthDegrees);
        if (Math.Abs(SignedCurvature) <= AlignmentMath.CurvatureTolerance)
        {
            var tangent = new TangentAlignmentElement(Name, StartState, Length);
            return tangent.GetStateAt(distance) with { ElementType = ElementType };
        }

        var endHeading = startHeading + SignedCurvature * distance;
        var x = StartState.X + (Math.Sin(endHeading) - Math.Sin(startHeading)) / SignedCurvature;
        var y = StartState.Y + (-Math.Cos(endHeading) + Math.Cos(startHeading)) / SignedCurvature;
        return new AlignmentState(
            StartChainage + distance,
            x,
            y,
            AlignmentMath.NormalizeAzimuth(AlignmentMath.ToDegrees(endHeading)),
            SignedCurvature,
            ElementType,
            Name);
    }
}
