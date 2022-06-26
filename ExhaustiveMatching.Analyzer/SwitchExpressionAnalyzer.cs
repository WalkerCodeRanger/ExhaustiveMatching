using System.Collections.Immutable;
using System.Linq;
using ExhaustiveMatching.Analyzer.Enums.Analysis;
using ExhaustiveMatching.Analyzer.Enums.Semantics;
using ExhaustiveMatching.Analyzer.Semantics;
using ExhaustiveMatching.Analyzer.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    internal static class SwitchExpressionAnalyzer
    {
        public static void Analyze(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchExpression)
        {
            var switchKind = IsExhaustive(context, switchExpression);
            if (!switchKind.IsExhaustive) return;

            ReportWhenGuardNotSupported(context, switchExpression);

            var switchOnType = context.GetExpressionType(switchExpression.GoverningExpression);

            if (switchOnType != null
                && switchOnType.IsEnum(context, out var enumType, out var nullable))
                AnalyzeSwitchOnEnum(context, switchExpression, enumType, nullable);
            else if (!switchKind.ThrowsInvalidEnum)
                AnalyzeSwitchOnClosed(context, switchExpression, switchOnType);

            // TODO report warning that throws invalid enum isn't checked for exhaustiveness
        }

        private static SwitchStatementKind IsExhaustive(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchExpression)
        {
            var discardArm = switchExpression.Arms.LastOrDefault(a => a.Pattern is DiscardPatternSyntax);

            // If there is no discard arm or it doesn't throw, we assume the
            // dev doesn't want an exhaustive match
            if (discardArm?.Expression is ThrowExpressionSyntax throwExpression)
                return ExpressionAnalyzer.SwitchStatementKindForThrown(context, throwExpression.Expression);

            return new SwitchStatementKind(false, false);
        }

        private static void ReportWhenGuardNotSupported(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchExpression)
        {
            foreach (var arm in switchExpression.Arms)
                if (arm.WhenClause != null)
                    context.ReportWhenClauseNotSupported(arm.WhenClause);
        }

        private static void AnalyzeSwitchOnEnum(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchExpression,
            ITypeSymbol enumType,
            bool nullRequired)
        {
            var patterns = switchExpression.Arms.Select(a => a.Pattern).ToList();

            // If null were not required, and there were a null case, that would already be a compile error
            if (nullRequired && !patterns.Any(PatternSyntaxExtensions.IsNullPattern))
                Diagnostics.ReportNotExhaustiveNullableEnumSwitch(context, switchExpression);

            var caseExpressions = patterns.OfType<ConstantPatternSyntax>().Select(p => p.Expression);
            var unusedSymbols = SwitchOnEnumAnalyzer.UnusedEnumValues(context, enumType, caseExpressions);
            Diagnostics.ReportNotExhaustiveEnumSwitch(context, switchExpression, unusedSymbols);
        }

        private static void AnalyzeSwitchOnClosed(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchExpression,
            ITypeSymbol type)
        {
            var patterns = switchExpression.Arms.Select(a => a.Pattern).ToList();

            var closedAttributeType = context.GetClosedAttributeType();
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

            var typesUsed = patterns
                .Select(pattern => pattern.GetMatchedTypeSymbol(context, type, allCases, isClosed))
                .Where(t => t != null) // returns null for invalid case clauses
                .ToImmutableHashSet();

            // If it is an open type, we don't want to actually check for uncovered types, but
            // we still needed to check the switch cases
            if (!isClosed)
            {
                context.ReportOpenTypeNotSupported(type, switchExpression.GoverningExpression);
                return; // No point in trying to check for uncovered types, this isn't closed
            }

            var uncoveredTypes = allConcreteTypes
                .Where(t => !typesUsed.Any(t.IsSubtypeOf))
                .ToArray();

            context.ReportNotExhaustiveObjectSwitch(switchExpression.SwitchKeyword, uncoveredTypes);
        }
    }
}
