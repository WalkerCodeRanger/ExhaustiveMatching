using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
                    var typeInfo = context.SemanticModel.GetTypeInfo(declarationPattern.Type,
                        context.CancellationToken);
                    symbolUsed = typeInfo.Type;
                    break;
                case DiscardPatternSyntax _:
                case ConstantPatternSyntax constantPattern
                    when constantPattern.IsNull():
                    // Ignored
                    return null;
                case VarPatternSyntax _:
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
