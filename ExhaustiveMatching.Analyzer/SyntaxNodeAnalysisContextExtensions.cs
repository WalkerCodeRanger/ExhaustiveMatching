using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    public static class SyntaxNodeAnalysisContextExtensions
    {
        #region Report Not Exhuastive Diagnostics
        public static void ReportNotExhaustiveEnumSwitch(
            this SyntaxNodeAnalysisContext context,
            SyntaxToken switchKeyword,
            string[] unusedSymbols)
        {
            foreach (var unusedSymbol in unusedSymbols.OrderBy(s => s))
            {
                var diagnostic = Diagnostic.Create(
                    ExhaustiveMatchAnalyzer.NotExhaustiveEnumSwitch,
                    switchKeyword.GetLocation(), unusedSymbol);
                context.ReportDiagnostic(diagnostic);
            }
        }

        public static void ReportNotExhaustiveNullableEnumSwitch(
            this SyntaxNodeAnalysisContext context,
            SyntaxToken switchKeyword)
        {
            var diagnostic = Diagnostic.Create(
                ExhaustiveMatchAnalyzer.NotExhaustiveNullableEnumSwitch,
                switchKeyword.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        public static void ReportNotExhaustiveObjectSwitch(
            this SyntaxNodeAnalysisContext context,
            SyntaxToken switchKeyword,
            ITypeSymbol[] uncoveredTypes)
        {
            foreach (var uncoveredType in uncoveredTypes.OrderBy(t => t.Name))
            {
                var diagnostic = Diagnostic.Create(
                    ExhaustiveMatchAnalyzer.NotExhaustiveObjectSwitch,
                    switchKeyword.GetLocation(), uncoveredType.GetFullName());
                context.ReportDiagnostic(diagnostic);
            }
        }
        #endregion

        #region Report Type Diagnostics
        #endregion

        #region Report Switch Diagnostics
        public static void ReportCasePatternNotSupported(
            this SyntaxNodeAnalysisContext context,
            CaseSwitchLabelSyntax switchLabel)
        {
            var diagnostic = Diagnostic.Create(
                ExhaustiveMatchAnalyzer.CasePatternNotSupported,
                switchLabel.Value.GetLocation(), switchLabel.Value.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        public static void ReportCasePatternNotSupported(this SyntaxNodeAnalysisContext context, PatternSyntax pattern)
        {
            var diagnostic = Diagnostic.Create(
                ExhaustiveMatchAnalyzer.CasePatternNotSupported,
                pattern.GetLocation(), pattern.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        public static void ReportOpenTypeNotSupported(
            this SyntaxNodeAnalysisContext context,
            ITypeSymbol type,
            ExpressionSyntax switchStatementExpression)
        {
            var diagnostic = Diagnostic.Create(
                ExhaustiveMatchAnalyzer.OpenTypeNotSupported,
                switchStatementExpression.GetLocation(), type.GetFullName());
            context.ReportDiagnostic(diagnostic);
        }

        public static void ReportWhenClauseNotSupported(
            this SyntaxNodeAnalysisContext context,
            WhenClauseSyntax whenClause)
        {
            var diagnostic = Diagnostic.Create(
                                ExhaustiveMatchAnalyzer.WhenGuardNotSupported,
                                whenClause.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
        #endregion

        public static ISymbol GetSymbol(
            this SyntaxNodeAnalysisContext context,
            ExpressionSyntax expression)
        {
            return context.SemanticModel
                          .GetSymbolInfo(expression, context.CancellationToken)
                          .Symbol;
        }

        public static INamedTypeSymbol GetClosedAttributeType(this SyntaxNodeAnalysisContext context)
        {
            return context.Compilation.GetTypeByMetadataName(TypeNames.ClosedAttribute);
        }
    }
}
