using System;
using ExhaustiveMatching.Analyzer.Enums.Semantics;
using ExhaustiveMatching.Analyzer.Enums.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums
{
    internal class EnumSwitchStatementAnalyzer
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, SwitchStatementSyntax switchStatement)
        {
            if (!IsExhaustive(context, switchStatement)) return;

            throw new NotImplementedException();

            //ReportWhenGuardNotSupported(context, switchStatement);

            //var switchOnType = context.GetExpressionType(switchStatement.Expression);

            //if (switchOnType != null && switchOnType.IsEnum(context, out var enumType, out var nullable))
            //    AnalyzeSwitchOnEnum(context, switchStatement, enumType, nullable);
            //else if (!switchKind.ThrowsInvalidEnum) AnalyzeSwitchOnClosed(context, switchStatement, switchOnType);

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
    }
}
