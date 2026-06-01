namespace SurveyCalcKit.Core.Models;

public sealed record ParseError(int LineNumber, string RawLine, string Message);
