namespace SurveyCalcKit.Core.Models;

public interface IAlignmentElement
{
    string Name { get; }
    string ElementType { get; }
    double StartChainage { get; }
    double Length { get; }
    AlignmentState StartState { get; }
    AlignmentState EndState { get; }

    AlignmentState GetStateAt(double localDistance);
}
