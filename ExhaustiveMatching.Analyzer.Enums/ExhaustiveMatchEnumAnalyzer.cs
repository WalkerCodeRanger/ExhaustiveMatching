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
        // You can change these strings in the Resources.resx file.
        private static readonly LocalizableString EM0001Title = LoadString(nameof(Resources.EM0001Title));
        private static readonly LocalizableString EM0001Message = LoadString(nameof(Resources.EM0001Message));
        private static readonly LocalizableString EM0001Description = LoadString(Resources.EM0001Description);

        private static readonly LocalizableString EM0002Title = LoadString(nameof(Resources.EM0002Title));
        private static readonly LocalizableString EM0002Message = LoadString(nameof(Resources.EM0002Message));
        private static readonly LocalizableString EM0002Description = LoadString(Resources.EM0002Description);

        private const string Category = "Logic";

        public static readonly DiagnosticDescriptor NotExhaustiveEnumSwitch = new DiagnosticDescriptor("EM0001",
            EM0001Title, EM0001Message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true,
            EM0001Description);

        public static readonly DiagnosticDescriptor NotExhaustiveNullableEnumSwitch = new DiagnosticDescriptor("EM0002",
            EM0002Title, EM0002Message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true,
            EM0002Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(NotExhaustiveEnumSwitch, NotExhaustiveNullableEnumSwitch);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze
                                                   | GeneratedCodeAnalysisFlags.ReportDiagnostics);
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
                throw new Exception($"Uncaught exception in analyzer: {exDetails}");
            }
        }

        private static LocalizableResourceString LoadString(string name)
            => new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
    }
}
