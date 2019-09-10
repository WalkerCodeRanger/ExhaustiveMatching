using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExhaustiveMatchAnalyzer : DiagnosticAnalyzer
    {
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString EM001Title = LoadString(nameof(Resources.EM001Title));
        private static readonly LocalizableString EM001Message = LoadString(nameof(Resources.EM001Message));
        private static readonly LocalizableString EM001Description = LoadString(Resources.EM001Description);

        private static readonly LocalizableString EM002Title = LoadString(nameof(Resources.EM001Title));
        private static readonly LocalizableString EM002Message = LoadString(nameof(Resources.EM001Message));
        private static readonly LocalizableString EM002Description = LoadString(Resources.EM001Description);

        private static readonly LocalizableString EM100Title = LoadString(nameof(Resources.EM100Title));
        private static readonly LocalizableString EM100Message = LoadString(nameof(Resources.EM100Message));
        private static readonly LocalizableString EM100Description = LoadString(Resources.EM100Description);

        private static readonly LocalizableString EM101Title = LoadString(nameof(Resources.EM101Title));
        private static readonly LocalizableString EM101Message = LoadString(nameof(Resources.EM101Message));
        private static readonly LocalizableString EM101Description = LoadString(Resources.EM101Description);

        private const string Category = "Logic";

        public static readonly DiagnosticDescriptor NotExhaustiveEnumSwitchRule =
            new DiagnosticDescriptor("EM001", EM001Title,
                EM001Message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true,
                EM001Description);

        public static readonly DiagnosticDescriptor NotExhaustiveObjectSwitchRule =
            new DiagnosticDescriptor("EM002", EM002Title, EM002Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM002Description);

        public static readonly DiagnosticDescriptor WhenClauseNotSupported =
            new DiagnosticDescriptor("EM100", EM100Title, EM100Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM100Description);

        public static readonly DiagnosticDescriptor UnsupportedCaseClauseType =
            new DiagnosticDescriptor("EM101", EM101Title, EM101Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM101Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(NotExhaustiveEnumSwitchRule,
                NotExhaustiveObjectSwitchRule, WhenClauseNotSupported,
                UnsupportedCaseClauseType);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            // TODO Analyze Class, Interface and Struct Declarations
            // A members of a Union Type must either be sealed/struct or union types
            // Any type inheriting from a union type must be listed in the union
        }

        private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is SwitchStatementSyntax switchStatement))
                throw new InvalidOperationException(
                    $"{nameof(AnalyzeSwitchStatement)} called with a non-switch statement context");

            SwitchStatementAnalyzer.Analyze(context, switchStatement);
        }

        private static LocalizableResourceString LoadString(string name)
        {
            return new LocalizableResourceString(name,
                Resources.ResourceManager, typeof(Resources));
        }
    }
}
