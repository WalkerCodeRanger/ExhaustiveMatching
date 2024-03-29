using System.Linq;
using ExhaustiveMatching.Analyzer.Enums.Analysis;
using ExhaustiveMatching.Analyzer.Enums.Semantics;
using ExhaustiveMatching.Analyzer.Enums.Syntax;
using ExhaustiveMatching.Analyzer.Enums.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums
{
    internal class EnumSwitchStatementAnalyzer
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, SwitchStatementSyntax switchStatement)
        {
            if (!IsExhaustive(context, switchStatement)) return;

            ReportCasePatternsNotSupported(context, switchStatement);

            var switchOnType = context.GetExpressionConvertedType(switchStatement.Expression);

            if (switchOnType != null && switchOnType.IsEnum(context, out var enumType, out var nullable))
                AnalyzeSwitchOnEnum(context, switchStatement, enumType, nullable);

            // TODO report warning that throws invalid enum isn't checked for exhaustiveness
        }

        private static bool IsExhaustive(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement)
        {
            // If there is no default section or it doesn't throw, we assume the
            // dev doesn't want an exhaustive match
            return switchStatement.DefaultSection()
                                  ?.FirstThrowStatement()
                                  ?.ThrowsType(context)
                                  ?.IsInvalidEnumArgumentException()
                   ?? false;
        }

        private static void ReportCasePatternsNotSupported(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement)
        {
            var unsupportedLabels = switchStatement.Labels().Where(l => !l.IsTraditional());
            foreach (var label in unsupportedLabels)
                Diagnostics.ReportCasePatternNotSupported(context, label);
        }

        private static void AnalyzeSwitchOnEnum(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            INamedTypeSymbol enumType,
            bool nullRequired)
        {
            var caseSwitchLabels = switchStatement.CaseSwitchLabels().ToReadOnlyList();

            // If null were not required, and there were a null case, that would already be a compile error
            if (nullRequired && !caseSwitchLabels.Any(l => l.IsNullCase()))
                Diagnostics.ReportNotExhaustiveNullableEnumSwitch(context, switchStatement);

            var caseExpressions = caseSwitchLabels.Select(l => l.Value);
            var unusedSymbols = SwitchOnEnumAnalyzer.UnusedEnumValues(context, enumType, caseExpressions);
            Diagnostics.ReportNotExhaustiveEnumSwitch(context, switchStatement, unusedSymbols);
        }
    }
}
