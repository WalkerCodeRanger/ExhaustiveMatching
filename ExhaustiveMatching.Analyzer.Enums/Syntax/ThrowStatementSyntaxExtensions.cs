using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums.Syntax
{
    public static class ThrowStatementSyntaxExtensions
    {
        /// <summary>
        /// The type of the expression being thrown.
        /// </summary>
        /// <returns>The type being thrown or <see langword="null"/> if it can't
        /// be determined (e.g. compile error).</returns>
        public static ITypeSymbol ThrowsType(this ThrowStatementSyntax throwStatement, SyntaxNodeAnalysisContext context)
        {
            var thrownExpression = throwStatement.Expression;
            var exceptionType = context.SemanticModel.GetTypeInfo(thrownExpression, context.CancellationToken).Type;
            if (exceptionType == null || exceptionType.TypeKind == TypeKind.Error)
                return null;

            return exceptionType;
        }
    }
}
