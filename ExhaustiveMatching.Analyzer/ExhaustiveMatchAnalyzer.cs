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
        // You can change these strings in the Resources.resx file.
        private static readonly LocalizableString EM001Title = LoadString(nameof(Resources.EM001Title));
        private static readonly LocalizableString EM001Message = LoadString(nameof(Resources.EM001Message));
        private static readonly LocalizableString EM001Description = LoadString(Resources.EM001Description);

        private static readonly LocalizableString EM002Title = LoadString(nameof(Resources.EM002Title));
        private static readonly LocalizableString EM002Message = LoadString(nameof(Resources.EM002Message));
        private static readonly LocalizableString EM002Description = LoadString(Resources.EM002Description);

        private static readonly LocalizableString EM011Title = LoadString(nameof(Resources.EM011Title));
        private static readonly LocalizableString EM011Message = LoadString(nameof(Resources.EM011Message));
        private static readonly LocalizableString EM011Description = LoadString(Resources.EM011Description);

        private static readonly LocalizableString EM012Title = LoadString(nameof(Resources.EM012Title));
        private static readonly LocalizableString EM012Message = LoadString(nameof(Resources.EM012Message));
        private static readonly LocalizableString EM012Description = LoadString(Resources.EM012Description);

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

        public static readonly DiagnosticDescriptor MustBeUnionMember =
            new DiagnosticDescriptor("EM011", EM011Title, EM011Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM011Description);

        public static readonly DiagnosticDescriptor MustBeDirectSubtype =
            new DiagnosticDescriptor("EM012", EM012Title, EM012Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM012Description);

        public static readonly DiagnosticDescriptor WhenClauseNotSupported =
            new DiagnosticDescriptor("EM100", EM100Title, EM100Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM100Description);

        public static readonly DiagnosticDescriptor UnsupportedCaseClauseType =
            new DiagnosticDescriptor("EM101", EM101Title, EM101Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM101Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(NotExhaustiveEnumSwitchRule,
                NotExhaustiveObjectSwitchRule,
                MustBeUnionMember, MustBeDirectSubtype, WhenClauseNotSupported,
                UnsupportedCaseClauseType);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.StructDeclaration);
        }

        private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is SwitchStatementSyntax switchStatement))
                throw new InvalidOperationException(
                    $"{nameof(AnalyzeSwitchStatement)} called with a non-switch statement context");

            SwitchStatementAnalyzer.Analyze(context, switchStatement);
        }

        private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is TypeDeclarationSyntax typeDeclaration))
                throw new InvalidOperationException(
                    $"{nameof(AnalyzeTypeDeclaration)} with a non-type declaration context");

            TypeDeclarationAnalyzer.Analyze(context, typeDeclaration);
        }

        private static LocalizableResourceString LoadString(string name)
        {
            return new LocalizableResourceString(name,
                Resources.ResourceManager, typeof(Resources));
        }
    }
}
