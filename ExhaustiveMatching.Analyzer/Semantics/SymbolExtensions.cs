using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis;

namespace ExhaustiveMatching.Analyzer.Semantics
{
    public static class SymbolExtensions
    {
        public static string GetFullName(this ISymbol symbol)
        {
            var ns = symbol.ContainingNamespace;
            return ns != null && !ns.IsGlobalNamespace ? $"{ns.GetFullName()}.{symbol.Name}" : symbol.Name;
        }

        /// <summary>Compares <see cref="ITypeSymbol"/> using <see cref="SymbolEqualityComparer.Default"/>, in accordance with <see href="https://github.com/dotnet/roslyn-analyzers/blob/cc67474108c080ea69af95bc193734a4dca65ee3/src/Microsoft.CodeAnalysis.Analyzers/Core/MetaAnalyzers/CompareSymbolsCorrectlyAnalyzer.cs#L17">RS1024</see>.</summary>
        public static bool EqualsDisregardingNullability(this ISymbol symbol, [NotNullWhen(true)] ISymbol? other)
        {
            return SymbolEqualityComparer.Default.Equals(symbol, other);
        }

        /// <summary>Compares <see cref="ITypeSymbol"/> using <see cref="SymbolEqualityComparer.IncludeNullability"/>, in accordance with <see href="https://github.com/dotnet/roslyn-analyzers/blob/cc67474108c080ea69af95bc193734a4dca65ee3/src/Microsoft.CodeAnalysis.Analyzers/Core/MetaAnalyzers/CompareSymbolsCorrectlyAnalyzer.cs#L17">RS1024</see>.</summary>
        public static bool EqualsConsideringNullability(this ISymbol symbol, [NotNullWhen(true)] ISymbol? other)
        {
            return SymbolEqualityComparer.IncludeNullability.Equals(symbol, other);
        }
    }
}
