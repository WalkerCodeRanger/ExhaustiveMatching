using ExhaustiveMatching.Analyzer.Enums.Semantics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Syntax
{
    public static class ExpressionSyntaxExtensions
    {
        public static bool IsTypeIdentifier(this ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out ITypeSymbol typeSymbol)
        {
            if (context.GetSymbol(expression) is ITypeSymbol t)
            {
                typeSymbol = t;
                return true;
            }

            typeSymbol = null;
            return false;
        }
    }
}
