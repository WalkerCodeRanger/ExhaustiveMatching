using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Semantics
{
    public static class SyntaxNodeAnalysisContextExtensions
    {
        public static ISymbol GetSymbol(this SyntaxNodeAnalysisContext context, AttributeSyntax attribute) =>
            context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol;
    }
}
