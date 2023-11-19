using ExhaustiveMatching.Analyzer.Enums.Semantics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums.Syntax
{
    public static class ThrowStatementSyntaxExtensions
    {
        /// <summary>
        /// Returns the <see cref="ITypeParameterSymbol"/> for type being thrown by <paramref name="throwStatement"/>, if determinable, otherwise <see langword="null"/>.
        /// </summary>
        /// <returns>The type being thrown or <see langword="null"/> if it can't
        /// be determined (e.g. compile error).</returns>
        public static ITypeSymbol? ThrowsType(this ThrowStatementSyntax throwStatement, SyntaxNodeAnalysisContext context)
        {
            if (throwStatement.Expression is null) return null;
            var exceptionType = context.GetExpressionType(throwStatement.Expression);
            if (exceptionType == null || exceptionType is IErrorTypeSymbol)
                return null;

            return exceptionType;
        }
    }
}
