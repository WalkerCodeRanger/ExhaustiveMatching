using Microsoft.CodeAnalysis;

namespace ExhaustiveMatching.Analyzer.Enums.Semantics
{
    public static class SymbolExtensions
    {
        public static string ToErrorDisplayString(this ISymbol symbol)
            => symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }
}
