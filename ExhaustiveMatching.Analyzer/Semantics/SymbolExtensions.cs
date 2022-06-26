using Microsoft.CodeAnalysis;

namespace ExhaustiveMatching.Analyzer
{
    public static class SymbolExtensions
    {
        public static string GetFullName(this ISymbol symbol)
        {
            var ns = symbol.ContainingNamespace;
            return ns != null && !ns.IsGlobalNamespace ? $"{ns.GetFullName()}.{symbol.Name}" : symbol.Name;
        }
    }
}
