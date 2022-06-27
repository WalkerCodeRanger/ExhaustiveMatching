using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer.Enums.Syntax
{
    public static class CaseSwitchLabelSyntaxExtensions
    {
        public static bool IsNullCase(this CaseSwitchLabelSyntax switchLabel)
            => switchLabel.Value.IsNullConstantExpression();
    }
}
