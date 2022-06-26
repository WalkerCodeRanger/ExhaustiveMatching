using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExhaustiveMatchAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => Diagnostics.SupportedDiagnostics;

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze|GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(AnalyzeSwitchExpression, SyntaxKind.SwitchExpression);
            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.StructDeclaration);
        }

        private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is SwitchStatementSyntax switchStatement))
                throw new InvalidOperationException(
                    $"{nameof(AnalyzeSwitchStatement)} called with a non-switch statement context");
            try
            {
                SwitchStatementAnalyzer.Analyze(context, switchStatement);
            }
            catch (Exception ex)
            {
                // Include stack trace info by ToString() the exception as part of the message.
                // Only the first line is included, so we have to remove newlines
                var exDetails = Regex.Replace(ex.ToString(), @"\r\n?|\n|\r", " ");
                throw new Exception($"Uncaught exception in analyzer: {exDetails}");
            }
        }

        private void AnalyzeSwitchExpression(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is SwitchExpressionSyntax switchExpression))
                throw new InvalidOperationException(
                    $"{nameof(AnalyzeSwitchExpression)} called with a non-switch expression context");
            try
            {
                SwitchExpressionAnalyzer.Analyze(context, switchExpression);
            }
            catch (Exception ex)
            {
                // Include stack trace info by ToString() the exception as part of the message.
                // Only the first line is included, so we have to remove newlines
                var exDetails = Regex.Replace(ex.ToString(), @"\r\n?|\n|\r", " ");
                throw new Exception($"Uncaught exception in analyzer: {exDetails}");
            }
        }

        private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is TypeDeclarationSyntax typeDeclaration))
                throw new InvalidOperationException(
                    $"{nameof(AnalyzeTypeDeclaration)} with a non-type declaration context");

            try
            {
                TypeDeclarationAnalyzer.Analyze(context, typeDeclaration);
            }
            catch (Exception ex)
            {
                // Include stack trace info by ToString() the exception as part of the message.
                // Only the first line is included, so we have to remove newlines
                var exDetails = Regex.Replace(ex.ToString(), @"\r\n?|\n|\r", " ");
                throw new Exception($"Uncaught exception in analyzer: {exDetails}");
            }
        }
    }
}
