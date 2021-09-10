using System;
using System.Linq;
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
            var valueOutOfRangeExceptionType =
                context.Compilation.GetTypeByMetadataName(TypeNames.ValueOutOfRangeException);
            var invalidEnumArgumentExceptionType =
                context.Compilation.GetTypeByMetadataName(TypeNames.InvalidEnumArgumentException);

            var isExhaustiveMatchFailedException = exceptionType.Equals(exhaustiveMatchFailedExceptionType, SymbolEqualityComparer.Default);
            var isValueOutOfRangeException = exceptionType.Equals(valueOutOfRangeExceptionType, SymbolEqualityComparer.Default);
            var isInvalidEnumArgumentException = exceptionType.Equals(invalidEnumArgumentExceptionType, SymbolEqualityComparer.Default);

            var dotNotValidate = false;

            if (isValueOutOfRangeException) {
                if (thrownExpression is ObjectCreationExpressionSyntax {
                    Initializer: { Expressions: { Count: 1 } } initializerExpression
                }) {
                    if (initializerExpression.Expressions[0] is AssignmentExpressionSyntax {
                        Left: IdentifierNameSyntax { Identifier: { Text: "DoNotValidate" } },
                        Right: LiteralExpressionSyntax { Token: { } tokenExpression }
                    }) {
                        if (tokenExpression.Kind() == SyntaxKind.TrueKeyword) {
                            dotNotValidate = true;
                        }
                    }
                }
            }

            var isExhaustive = isExhaustiveMatchFailedException || isValueOutOfRangeException || isInvalidEnumArgumentException;

            return new SwitchStatementKind(isExhaustive && !dotNotValidate, isInvalidEnumArgumentException);
        }
    }
}
