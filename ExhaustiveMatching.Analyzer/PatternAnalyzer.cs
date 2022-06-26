using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    internal static class PatternAnalyzer
    {
        public static ITypeSymbol GetMatchedTypeSymbol(
            this SwitchLabelSyntax switchLabel,
            SyntaxNodeAnalysisContext context,
            ITypeSymbol type,
            HashSet<ITypeSymbol> allCases,
            bool isClosed)
        {
            switch (switchLabel)
            {
                case CaseSwitchLabelSyntax labelSyntax:
                    return labelSyntax.Value.IsTypeIdentifier(context, out var result) ? result : null;
                case CasePatternSwitchLabelSyntax patternSyntax:
                    return patternSyntax.Pattern.GetMatchedTypeSymbol(context, type, allCases, isClosed);
                default:
                    return null;
            }
        }

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
                case ConstantPatternSyntax constantTypePattern
                    when constantTypePattern.Expression.IsTypeIdentifier(context, out symbolUsed):
                    break;
                case DiscardPatternSyntax _:
                case ConstantPatternSyntax constantPattern
                    when constantPattern.IsNullPattern():
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
