using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    public static class SwitchStatementAnalyzer
    {
        public static void Analyze(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement)
        {
            var switchKind = IsExhaustive(context, switchStatement);
            if (!switchKind.IsExhaustive) return;

            ReportWhenGuardNotSupported(context, switchStatement);

            var switchOnType = context.GetExpressionType(switchStatement.Expression);

            if (switchOnType != null
                && switchOnType.IsEnum(context, out var enumType, out var nullable))
                AnalyzeSwitchOnEnum(context, switchStatement, enumType, nullable);
            else if (!switchKind.ThrowsInvalidEnum)
                AnalyzeSwitchOnClosed(context, switchStatement, switchOnType);

            // TODO report warning that throws invalid enum isn't checked for exhaustiveness
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

        private static SwitchStatementKind IsExhaustive(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement)
        {
            var defaultSection = switchStatement.Sections
                .FirstOrDefault(s => s.Labels.OfType<DefaultSwitchLabelSyntax>().Any());

            var defaultSectionStatements = defaultSection?.Statements;

            var defaultSectionBlock = defaultSectionStatements?.OfType<BlockSyntax>().FirstOrDefault();
            if (defaultSectionBlock != null)
                defaultSectionStatements = defaultSectionBlock.Statements;

            var throwStatement = defaultSectionStatements?
                                    .OfType<ThrowStatementSyntax>().FirstOrDefault();

            // If there is no default section or it doesn't throw, we assume the
            // dev doesn't want an exhaustive match
            if (throwStatement != null)
                return ExpressionAnalyzer.SwitchStatementKindForThrown(context,
                    throwStatement.Expression);

            return new SwitchStatementKind(false, false);
        }

        private static void AnalyzeSwitchOnEnum(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            ITypeSymbol type,
            bool nullRequired = false)
        {
            var caseSwitchLabels = switchStatement
                                    .Sections
                                    .SelectMany(s => s.Labels)
                                    .OfType<CaseSwitchLabelSyntax>()
                                    .ToList();

            var symbolsUsed = caseSwitchLabels
                              .Select(l => context.GetSymbol(l.Value))
                              .ToImmutableHashSet(SymbolEqualityComparer.Default);

            // If null were not required, and there were a null case, that would already be a compile error
            if (nullRequired && !caseSwitchLabels.Any(IsNullCase))
                context.ReportNotExhaustiveNullableEnumSwitch(switchStatement.SwitchKeyword);

            var allSymbols = type.GetMembers()
                                 .Where(m => m.Kind == SymbolKind.Field)
                                 .ToArray();

            var unusedSymbols = allSymbols
                                // Use where instead of Except because we have a dictionary
                                .Where(m => !symbolsUsed.Contains(m))
                                .Select(m => m.Name)
                                .ToArray();

            context.ReportNotExhaustiveEnumSwitch(switchStatement.SwitchKeyword, unusedSymbols);
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
            var isClosed = type.HasAttribute(closedAttributeType);

            var allCases = type.GetClosedTypeCases(closedAttributeType);

            var typesUsed = switchLabels
                .OfType<CasePatternSwitchLabelSyntax>()
                .Select(patternLabel => patternLabel.Pattern.GetMatchedTypeSymbol(context, type, allCases, isClosed))
                .Where(t => t != null) // returns null for invalid case clauses
                .ToImmutableHashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            // If it is an open type, we don't want to actually check for uncovered types, but
            // we still needed to check the switch cases
            if (!isClosed)
            {
                context.ReportOpenTypeNotSupported(type, switchStatement.Expression);
                return; // No point in trying to check for uncovered types, this isn't closed
            }

            var uncoveredTypes = allCases
                .Where(t => t.IsConcreteOrLeaf(closedAttributeType))
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
                if (!IsNullCase(switchLabel))
                    context.ReportCasePatternNotSupported(switchLabel);
                // `case null:` is allowed
            }
        }

        private static bool IsNullCase(CaseSwitchLabelSyntax switchLabel)
        {
            return switchLabel.Value.IsNullLiteral();
        }
    }
}
