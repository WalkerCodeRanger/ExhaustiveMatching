using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer.Enums.Syntax
{
    public static class ExpressionSyntaxExtensions
    {
        public static bool IsNullLiteral(this ExpressionSyntax expression)
            => expression is LiteralExpressionSyntax literalExpression
               && literalExpression.Kind() == SyntaxKind.NullLiteralExpression;
    }
}
