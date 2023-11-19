using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ExhaustiveMatching.Analyzer.Enums.Analysis;
using ExhaustiveMatching.Analyzer.Enums.Semantics;
using ExhaustiveMatching.Analyzer.Enums.Syntax;
using ExhaustiveMatching.Analyzer.Enums.Utility;
using ExhaustiveMatching.Analyzer.Semantics;
using ExhaustiveMatching.Analyzer.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    internal static class SwitchStatementAnalyzer
    {
        public static void Analyze(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement)
        {
            var switchKind = IsExhaustive(context, switchStatement);
            if (!switchKind.IsExhaustive) return;

            ReportWhenGuardNotSupported(context, switchStatement);

            var switchOnType = context.GetExpressionConvertedType(switchStatement.Expression);
            if (switchOnType != null)
            {
                if (switchOnType.IsEnum(context, out var enumType, out var nullable))
                {
                    AnalyzeSwitchOnEnum(context, switchStatement, enumType, nullable);
                }
                else if (!switchKind.ThrowsInvalidEnum)
                {
                    AnalyzeSwitchOnClosed(context, switchStatement, switchOnType);
                }
            }

            // TODO report warning that `throws <invalid enum>` isn't checked for exhaustiveness
        }

        private static SwitchStatementKind IsExhaustive(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement)
        {
            var defaultSection = switchStatement.Sections
                .FirstOrDefault(s => s.Labels.OfType<DefaultSwitchLabelSyntax>().Any());

            var throwStatement = defaultSection?.Statements.OfType<ThrowStatementSyntax>().FirstOrDefault();

            // If there is no default section or it doesn't throw, we assume the
            // dev doesn't want an exhaustive match
            if (throwStatement == null || throwStatement.Expression is null)
            {
                return new SwitchStatementKind(isExhaustive: false, throwsInvalidEnum: false);
            }

            return ExpressionAnalyzer.SwitchStatementKindForThrown(context, throwStatement.Expression);
        }

        private static void ReportWhenGuardNotSupported(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement)
        {
            var patternLabels = switchStatement.Sections.SelectMany(s => s.Labels)
                                               .OfType<CasePatternSwitchLabelSyntax>();
            foreach (var patternLabel in patternLabels)
                if (patternLabel.WhenClause != null)
                    context.ReportWhenClauseNotSupported(patternLabel.WhenClause);
        }

        private static void AnalyzeSwitchOnEnum(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            INamedTypeSymbol enumType,
            bool nullRequired)
        {
            var caseSwitchLabels = switchStatement.CaseSwitchLabels().ToReadOnlyList();

            // If null were not required, and there were a null case, that would already be a compile error
            if (nullRequired && !caseSwitchLabels.Any(CaseSwitchLabelSyntaxExtensions.IsNullCase))
                Diagnostics.ReportNotExhaustiveNullableEnumSwitch(context, switchStatement);

            var caseExpressions = caseSwitchLabels.Select(l => l.Value);
            var unusedSymbols = SwitchOnEnumAnalyzer.UnusedEnumValues(context, enumType, caseExpressions);
            Diagnostics.ReportNotExhaustiveEnumSwitch(context, switchStatement, unusedSymbols);
        }

        private static void AnalyzeSwitchOnClosed(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            ITypeSymbol type)
        {
            var switchLabels = switchStatement
                .Sections.SelectMany(s => s.Labels).ToList();

            CheckForNonPatternCases(context, switchLabels);

            var closedAttributeType = context.GetClosedAttributeType();
            if (closedAttributeType is null) return;

            var isClosed = type.HasAttribute(closedAttributeType);

            var allCases = type.GetClosedTypeCases(closedAttributeType);
            var allConcreteTypes = allCases
                .Where(t => t.IsConcreteOrLeaf(closedAttributeType));

            if (!isClosed && type.TryGetStructurallyClosedTypeCases(context, out allCases))
            {
                isClosed = true;
                allConcreteTypes = allCases
                    .Where(t => t.IsConcrete());
            }

            var typesUsed = switchLabels
                .Select(switchLabel => switchLabel.GetMatchedTypeSymbol(context, type, allCases, isClosed)) // returns null for invalid case clauses
                .WhereNotNull()
                .ToImmutableHashSet();

            // If it is an open type, we don't want to actually check for uncovered types, but
            // we still needed to check the switch cases
            if (!isClosed)
            {
                context.ReportOpenTypeNotSupported(type, switchStatement.Expression);
                return; // No point in trying to check for uncovered types, this isn't closed
            }

            var uncoveredTypes = allConcreteTypes
                .Where(t => !typesUsed.Any(t.IsSubtypeOf))
                .ToArray();

            context.ReportNotExhaustiveObjectSwitch(switchStatement.SwitchKeyword, uncoveredTypes);
        }

        private static void CheckForNonPatternCases(
            SyntaxNodeAnalysisContext context,
            List<SwitchLabelSyntax> switchLabels)
        {
            foreach (var switchLabel in switchLabels.OfType<CaseSwitchLabelSyntax>())
            {
                if (!switchLabel.IsNullCase() && !switchLabel.Value.IsTypeIdentifier(context, out _))
                    context.ReportCasePatternNotSupported(switchLabel);
                // `case null:` is allowed
            }
        }
    }
}
