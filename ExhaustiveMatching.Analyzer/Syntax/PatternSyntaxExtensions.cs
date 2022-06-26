using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer
{
    internal static class PatternSyntaxExtensions
    {
        public static bool IsNullPattern(this PatternSyntax pattern)
        {
            return pattern is ConstantPatternSyntax constantPattern
                   && constantPattern.Expression.IsNullLiteral();
        }
    }
}
