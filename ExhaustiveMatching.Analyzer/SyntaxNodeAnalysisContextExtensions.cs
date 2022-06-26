using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    public static class SyntaxNodeAnalysisContextExtensions
    {
        #region Report Not Exhuastive Diagnostics
        public static void ReportNotExhaustiveObjectSwitch(
            this SyntaxNodeAnalysisContext context,
            SyntaxToken switchKeyword,
            ITypeSymbol[] uncoveredTypes)
        {
            foreach (var uncoveredType in uncoveredTypes.OrderBy(t => t.Name))
            {
                var diagnostic = Diagnostic.Create(
                    Diagnostics.NotExhaustiveObjectSwitch,
                    switchKeyword.GetLocation(), uncoveredType.GetFullName());
                context.ReportDiagnostic(diagnostic);
            }
        }
        #endregion

        #region Report Switch Diagnostics
        public static void ReportCasePatternNotSupported(
            this SyntaxNodeAnalysisContext context,
            CaseSwitchLabelSyntax switchLabel)
        {
            var diagnostic = Diagnostic.Create(
                Diagnostics.CasePatternNotSupported,
                switchLabel.Value.GetLocation(), switchLabel.Value.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        public static void ReportCasePatternNotSupported(this SyntaxNodeAnalysisContext context, PatternSyntax pattern)
        {
            var diagnostic = Diagnostic.Create(
                Diagnostics.CasePatternNotSupported,
                pattern.GetLocation(), pattern.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        public static void ReportOpenTypeNotSupported(
            this SyntaxNodeAnalysisContext context,
            ITypeSymbol type,
            ExpressionSyntax switchStatementExpression)
        {
            var diagnostic = Diagnostic.Create(
                Diagnostics.OpenTypeNotSupported,
                switchStatementExpression.GetLocation(), type.GetFullName());
            context.ReportDiagnostic(diagnostic);
        }

        public static void ReportWhenClauseNotSupported(
            this SyntaxNodeAnalysisContext context,
            WhenClauseSyntax whenClause)
        {
            var diagnostic = Diagnostic.Create(
                                Diagnostics.WhenGuardNotSupported,
                                whenClause.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
        #endregion

        public static INamedTypeSymbol GetClosedAttributeType(this SyntaxNodeAnalysisContext context)
            => context.Compilation.GetTypeByMetadataName(TypeNames.ClosedAttribute);

        public static ITypeSymbol GetDeclarationType(
            this SyntaxNodeAnalysisContext context,
            DeclarationPatternSyntax declarationPattern)
            => context.SemanticModel.GetTypeInfo(declarationPattern.Type, context.CancellationToken).Type;
    }
}
