using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        private static readonly LocalizableString EM100Title = LoadString(nameof(Resources.EM100Title));
        private static readonly LocalizableString EM100Message = LoadString(nameof(Resources.EM100Message));
        private static readonly LocalizableString EM100Description = LoadString(Resources.EM100Description);

        private static readonly LocalizableString EM101Title = LoadString(nameof(Resources.EM101Title));
        private static readonly LocalizableString EM101Message = LoadString(nameof(Resources.EM101Message));
        private static readonly LocalizableString EM101Description = LoadString(Resources.EM101Description);

        private const string Category = "Logic";

        private static readonly DiagnosticDescriptor NotExhaustiveEnumSwitchRule =
            new DiagnosticDescriptor("EM001", EM001Title,
                EM001Message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true,
                EM001Description);

        private static readonly DiagnosticDescriptor WhenClauseNotSupported =
            new DiagnosticDescriptor("EM100", EM100Title, EM100Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM100Description);

        private static readonly DiagnosticDescriptor UnsupportedCaseClauseType =
            new DiagnosticDescriptor("EM101", EM101Title, EM101Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM101Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(NotExhaustiveEnumSwitchRule, WhenClauseNotSupported,
                UnsupportedCaseClauseType);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is SwitchStatementSyntax switchStatement))
                throw new InvalidOperationException(
                    "Switch AnalyzeSwitchStatement called with a non-switch statement context");

            var switchKind = IsExhaustive(context, switchStatement);
            if (!switchKind.IsExhaustive)
                return;

            var expressionTypeInfo = context.SemanticModel
                .GetTypeInfo(switchStatement.Expression, context.CancellationToken);

            if (expressionTypeInfo.Type?.TypeKind == TypeKind.Enum)
                AnalyzeEnumSwitchStatement(context, switchStatement, expressionTypeInfo.Type);
            else if (!switchKind.DefaultThrowsInvalidEnum)
                AnalyzeObjectSwitchStatement(context, switchStatement, expressionTypeInfo.Type);
        }

        private static SwitchStatementKind IsExhaustive(SyntaxNodeAnalysisContext context, SwitchStatementSyntax switchStatement)
        {
            var defaultSection = switchStatement.Sections.FirstOrDefault(s =>
                s.Labels.OfType<DefaultSwitchLabelSyntax>().Any());

            var throwStatement = defaultSection?.Statements.OfType<ThrowStatementSyntax>().FirstOrDefault();

            // If there is no default section or it doesn't throw, we assume the
            // dev doesn't want an exhaustive match
            if (throwStatement != null)
            {
                var exceptionType = context.SemanticModel.GetTypeInfo(throwStatement.Expression, context.CancellationToken).Type;
                if (exceptionType != null)
                {
                    var exhaustiveMatchFailedExceptionType = context.Compilation.GetTypeByMetadataName("ExhaustiveMatching.ExhaustiveMatchFailedException");
                    var invalidEnumArgumentExceptionType = context.Compilation.GetTypeByMetadataName("System.ComponentModel.InvalidEnumArgumentException");

                    var isExhaustiveMatchFailedException = exceptionType.Equals(exhaustiveMatchFailedExceptionType);
                    var isInvalidEnumArgumentException = exceptionType.Equals(invalidEnumArgumentExceptionType);
                    var isExhaustive = isExhaustiveMatchFailedException || isInvalidEnumArgumentException;

                    return new SwitchStatementKind(isExhaustive, isInvalidEnumArgumentException);
                }
            }

            return new SwitchStatementKind(false, false);
        }

        private static void AnalyzeEnumSwitchStatement(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            ITypeSymbol type)
        {
            var symbolsUsed = switchStatement
                .Sections
                .SelectMany(s => s.Labels)
                .OfType<CaseSwitchLabelSyntax>()
                .Select(l => context.SemanticModel.GetSymbolInfo(l.Value, context.CancellationToken).Symbol)
                .ToImmutableHashSet();

            var allSymbols = type.GetMembers()
                .Where(m => m.Kind == SymbolKind.Field)
                .ToArray();

            var unusedSymbols = allSymbols
                    .Where(m => !symbolsUsed.Contains(m))
                    .Select(m => type.Name + "." + m.Name)
                    .ToArray();

            if (unusedSymbols.Any())
            {
                var diagnostic = Diagnostic.Create(NotExhaustiveEnumSwitchRule,
                    switchStatement.GetLocation(),
                    string.Join("\n", unusedSymbols));
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeObjectSwitchStatement(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            ITypeSymbol type)
        {
            // TODO check and report error for type does not have UnionOfTypes attribute
            // `case var x when x is Square:` is a VarPattern

            var switchLabels = switchStatement
                .Sections.SelectMany(s => s.Labels).ToList();

            CheckForNonPatternCases(context, switchLabels);

            var symbolsUsed = switchLabels
                .OfType<CasePatternSwitchLabelSyntax>()
                .Select(casePattern => GetTypeSymbolMatched(context, casePattern))
                .ToImmutableHashSet();

            // GetTypeSymbolMatched returns null for fatal errors that prevent exhaustiveness checking
            if (symbolsUsed.Contains(default))
                return;

            var allSymbols = GetAllConcreteUnionTypeMembers(context, type);
            // TODO get all subtypes and check that they are covered
            // use `type.ContainingAssembly.Accept()` and make a symbol visitor to get all types (watch out for nested classes)

            throw new NotImplementedException();
        }

        private static void CheckForNonPatternCases(
            SyntaxNodeAnalysisContext context,
            List<SwitchLabelSyntax> switchLabels)
        {
            foreach (var switchLabel in switchLabels.OfType<CaseSwitchLabelSyntax>())
            {
                if (switchLabel.Value is LiteralExpressionSyntax literalExpression
                    && literalExpression.Kind() == SyntaxKind.NullLiteralExpression)
                {
                    // `case null:` is allowed
                }
                else
                {
                    var diagnostic = Diagnostic.Create(UnsupportedCaseClauseType,
                        switchLabel.GetLocation(), switchLabel.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static ITypeSymbol GetTypeSymbolMatched(
            SyntaxNodeAnalysisContext context,
            CasePatternSwitchLabelSyntax casePattern)
        {
            ITypeSymbol symbolUsed;
            if (casePattern.Pattern is DeclarationPatternSyntax declarationPattern)
            {
                var typeInfo = context.SemanticModel.GetTypeInfo(declarationPattern.Type,
                    context.CancellationToken);
                symbolUsed = typeInfo.Type;
            }
            else
            {
                var diagnostic = Diagnostic.Create(UnsupportedCaseClauseType,
                    casePattern.GetLocation(), casePattern.ToString());
                context.ReportDiagnostic(diagnostic);
                symbolUsed = null;
            }

            if (casePattern.WhenClause != null)
            {
                var diagnostic = Diagnostic.Create(WhenClauseNotSupported,
                    casePattern.GetLocation(), casePattern.ToString());
                context.ReportDiagnostic(diagnostic);
            }

            return symbolUsed;
        }

        private static ITypeSymbol[] GetAllConcreteUnionTypeMembers(
            SyntaxNodeAnalysisContext context,
            ITypeSymbol type)
        {
            var unionOfTypesAttributeType
                = context.Compilation.GetTypeByMetadataName("ExhaustiveMatching.UnionOfTypesAttribute");
            var args = type.GetAttributes()
                .Where(a => a.AttributeClass.Equals(unionOfTypesAttributeType))
                .SelectMany(a => a.ConstructorArguments)
                .SelectMany(arg => arg.Values)
                .Select(arg => arg.Value)
                .Cast<Type>()
                .Select(t => context.Compilation.GetTypeByMetadataName(t.FullName))
                .ToArray();

            throw new NotImplementedException();
        }

        private static LocalizableResourceString LoadString(string name)
        {
            return new LocalizableResourceString(name,
                Resources.ResourceManager, typeof(Resources));
        }
    }
}
