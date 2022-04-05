using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    public static class ExpressionSyntaxExtensions
    {
        public static bool IsNullLiteral(this ExpressionSyntax expression)
        {
            return expression is LiteralExpressionSyntax literalExpression
                   && literalExpression.Kind() == SyntaxKind.NullLiteralExpression;
        }

        public static bool IsTypeIdentifier(this ExpressionSyntax expression, SyntaxNodeAnalysisContext context, out ITypeSymbol typeSymbol)
        {
            if (expression is IdentifierNameSyntax identifierName
                && context.GetSymbol(identifierName) is ITypeSymbol t)
            {
                typeSymbol = t;
                return true;
            }

            typeSymbol = null;
            return false;
        }
    }
}
