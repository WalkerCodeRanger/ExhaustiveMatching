using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer
{
    public static class ExpressionSyntaxExtensions
    {
        public static bool IsNullLiteral(this ExpressionSyntax expression)
        {
            return expression is LiteralExpressionSyntax literalExpression
                   && literalExpression.Kind() == SyntaxKind.NullLiteralExpression;
        }
    }
}
