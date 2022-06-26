using System.Collections.Generic;
using System.Linq;
using ExhaustiveMatching.Analyzer.Enums.Semantics;
using ExhaustiveMatching.Analyzer.Enums.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums.Analysis
{
    public static class SwitchOnEnumAnalyzer
    {
        public static IEnumerable<ISymbol> UnusedEnumValues(
            SyntaxNodeAnalysisContext context,
            ITypeSymbol enumType,
            IReadOnlyList<CaseSwitchLabelSyntax> caseSwitchLabels)
        {
            var symbolsUsed = caseSwitchLabels.Select(l => context.GetSymbol(l.Value)).ToHashSet();

            var allSymbols = enumType.GetMembers().Where(m => m.Kind == SymbolKind.Field).ToArray();

            // Use where instead of Except because we have a set
            return allSymbols.Where(m => !symbolsUsed.Contains(m));
        }
    }
}
