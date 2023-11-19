using ExhaustiveMatching.Analyzer.Semantics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    internal static class ExpressionAnalyzer
    {
        public static SwitchStatementKind SwitchStatementKindForThrown(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax thrownExpression)
        {
            var exceptionType = context.SemanticModel.GetTypeInfo(thrownExpression, context.CancellationToken).Type;
            if (exceptionType == null || exceptionType.TypeKind == TypeKind.Error)
                return new SwitchStatementKind(isExhaustive: false, throwsInvalidEnum: false);

            // TODO GetTypeByMetadataName returns null if multiple types match. This isn't the way to do this
            var exhaustiveMatchFailedExceptionType =
                context.Compilation.GetTypeByMetadataName(TypeNames.ExhaustiveMatchFailedException);
            var invalidEnumArgumentExceptionType =
                context.Compilation.GetTypeByMetadataName(TypeNames.InvalidEnumArgumentException);

            var isExhaustiveMatchFailedException = exceptionType.EqualsConsideringNullability(exhaustiveMatchFailedExceptionType);
            var isInvalidEnumArgumentException = exceptionType.EqualsConsideringNullability(invalidEnumArgumentExceptionType);
            var isExhaustive = isExhaustiveMatchFailedException || isInvalidEnumArgumentException;

            return new SwitchStatementKind(isExhaustive, isInvalidEnumArgumentException);
        }
    }
}
