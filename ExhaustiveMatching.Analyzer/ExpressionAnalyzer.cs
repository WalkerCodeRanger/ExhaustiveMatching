using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    public static class ExpressionAnalyzer
    {
        public static SwitchStatementKind SwitchStatementKindForThrown(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax thrownExpression)
        {
            var exceptionType = context.SemanticModel.GetTypeInfo(thrownExpression, context.CancellationToken).Type;
            if (exceptionType == null || exceptionType.TypeKind == TypeKind.Error)
                return new SwitchStatementKind(false, false);

            var exhaustiveMatchFailedExceptionType =
                context.Compilation.GetTypeByMetadataName(TypeNames.ExhaustiveMatchFailedException);
            var invalidEnumArgumentExceptionType =
                context.Compilation.GetTypeByMetadataName(TypeNames.InvalidEnumArgumentException);

            var isExhaustiveMatchFailedException = exceptionType.Equals(exhaustiveMatchFailedExceptionType, SymbolEqualityComparer.Default);
            var isInvalidEnumArgumentException = exceptionType.Equals(invalidEnumArgumentExceptionType, SymbolEqualityComparer.Default);
            var isExhaustive = isExhaustiveMatchFailedException || isInvalidEnumArgumentException;

            return new SwitchStatementKind(isExhaustive, isInvalidEnumArgumentException);
        }
    }
}
