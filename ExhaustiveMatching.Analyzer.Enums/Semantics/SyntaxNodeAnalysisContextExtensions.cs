using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums.Semantics
{
    public static class SyntaxNodeAnalysisContextExtensions
    {
        /// <summary>
        /// Get the type of an expression.
        /// </summary>
        public static ITypeSymbol GetExpressionType(
            this SyntaxNodeAnalysisContext context,
            ExpressionSyntax switchStatementExpression)
            => context.SemanticModel.GetTypeInfo(switchStatementExpression, context.CancellationToken).Type;

        /// <summary>
        /// Get the converted type of an expression.
        /// </summary>
        public static ITypeSymbol GetExpressionConvertedType(
            this SyntaxNodeAnalysisContext context,
            ExpressionSyntax switchStatementExpression) =>
            context.SemanticModel.GetTypeInfo(switchStatementExpression, context.CancellationToken).ConvertedType;

        public static ISymbol GetSymbol(this SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
            => context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol;
    }
}
