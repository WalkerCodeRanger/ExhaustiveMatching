using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer.Enums.Syntax
{
    public static class ExpressionSyntaxExtensions
    {
        public static bool IsNullLiteral(this ExpressionSyntax expression)
            => expression is LiteralExpressionSyntax literalExpression
               && literalExpression.Kind() == SyntaxKind.NullLiteralExpression;

        public static bool IsNullConstantExpression(this ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literalExpression when literalExpression.Kind() == SyntaxKind.NullLiteralExpression:
                    return true;
                case CastExpressionSyntax castExpression:
                    return castExpression.Expression.IsNullConstantExpression();
                default:
                    return false;
            }
        }
    }
}
