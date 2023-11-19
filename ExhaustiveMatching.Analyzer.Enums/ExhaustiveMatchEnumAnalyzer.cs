using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExhaustiveMatchEnumAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => Diagnostics.SupportedDiagnostics;

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            //context.RegisterSyntaxNodeAction(AnalyzeSwitchExpression, SyntaxKind.SwitchExpression);
        }

        private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is SwitchStatementSyntax switchStatement))
                throw new InvalidOperationException(
                    $"{nameof(AnalyzeSwitchStatement)} called with a non-switch statement context");
            try
            {
                EnumSwitchStatementAnalyzer.Analyze(context, switchStatement);
            }
            catch (Exception ex)
            {
                // Include stack trace info by ToString() the exception as part of the message.
                // Only the first line is included, so we have to remove newlines
                var exDetails = Regex.Replace(ex.ToString(), @"\r\n?|\n|\r", " ");
                throw new Exception($"Uncaught exception in analyzer: {exDetails}", innerException: ex);
            }
        }
    }
}
