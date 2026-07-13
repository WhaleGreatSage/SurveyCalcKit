using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class TangentAlignmentElement : IAlignmentElement
{
    public TangentAlignmentElement(string name, AlignmentState startState, double length)
    {
        Name = name;
        StartState = startState;
        Length = length;
        StartChainage = startState.Chainage;
        EndState = GetStateAt(length);
    }

    public string Name { get; }
    public string ElementType => "TANGENT";
    public double StartChainage { get; }
    public double Length { get; }
    public AlignmentState StartState { get; }
    public AlignmentState EndState { get; }

    public AlignmentState GetStateAt(double localDistance)
    {
        var distance = Math.Clamp(localDistance, 0, Length);
        var radians = AlignmentMath.ToRadians(StartState.AzimuthDegrees);
        return new AlignmentState(
            StartChainage + distance,
            StartState.X + distance * Math.Cos(radians),
            StartState.Y + distance * Math.Sin(radians),
            AlignmentMath.NormalizeAzimuth(StartState.AzimuthDegrees),
            0,
            ElementType,
            Name);
    }
}
