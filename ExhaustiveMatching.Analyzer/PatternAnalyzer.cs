using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    internal static class PatternAnalyzer
    {
        public static ITypeSymbol GetMatchedTypeSymbol(
            this PatternSyntax pattern,
            SyntaxNodeAnalysisContext context,
            ITypeSymbol type,
            HashSet<ITypeSymbol> allCases,
            bool isClosed)
        {
            ITypeSymbol symbolUsed;
            switch (pattern)
            {
                case DeclarationPatternSyntax declarationPattern:
                    symbolUsed = context.GetDeclarationType(declarationPattern);
                    break;
                case VarPatternSyntax _:
                case DiscardPatternSyntax _:
                    // Ignored
                    return null;
                case ConstantPatternSyntax constantPattern:
                    if (constantPattern.IsNull()) {
                        // Ignored
                        return null;
                    }

                    var patternSymbol = context.SemanticModel.GetSymbolInfo(constantPattern.Expression).Symbol as ITypeSymbol;
                    if (patternSymbol == null) {
                        // Ignored
                        return null;
                    }

                    symbolUsed = patternSymbol;
                    break;
                case RecursivePatternSyntax _:
                default:
                    context.ReportCasePatternNotSupported(pattern);
                    return null;
            }

            if (isClosed && !allCases.Contains(symbolUsed))
            {
                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.MatchMustBeOnCaseType,
                    pattern.GetLocation(), symbolUsed.GetFullName(), type.GetFullName());
                context.ReportDiagnostic(diagnostic);
            }

            return symbolUsed;
        }
    }
}
