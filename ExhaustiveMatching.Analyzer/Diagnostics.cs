using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    internal static class Diagnostics
    {
        public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(NotExhaustiveEnumSwitch, NotExhaustiveNullableEnumSwitch,
                NotExhaustiveObjectSwitch, ConcreteSubtypeMustBeCaseOfClosedType,
                MustBeDirectSubtype, MustBeSubtype, SubtypeMustBeCovered,
                OpenInterfaceSubtypeMustBeCaseOfClosedType, WhenGuardNotSupported,
                CasePatternNotSupported, OpenTypeNotSupported,
                MatchMustBeOnCaseType, DuplicateClosedAttribute, DuplicateCaseType);

        public static void ReportNotExhaustiveEnumSwitch(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            IEnumerable<ISymbol> unusedSymbols)
            => ReportNotExhaustiveEnumSwitch(context, switchStatement.SwitchKeyword, unusedSymbols);

        public static void ReportNotExhaustiveEnumSwitch(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchExpression,
            IEnumerable<ISymbol> unusedSymbols)
            => ReportNotExhaustiveEnumSwitch(context, switchExpression.SwitchKeyword, unusedSymbols);

        private static void ReportNotExhaustiveEnumSwitch(
            SyntaxNodeAnalysisContext context,
            SyntaxToken switchKeyword,
            IEnumerable<ISymbol> unusedSymbols)
        {
            foreach (var unusedSymbol in unusedSymbols.OrderBy(s => s))
            {
                var symbolName = unusedSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                var diagnostic = Diagnostic.Create(NotExhaustiveEnumSwitch, switchKeyword.GetLocation(), symbolName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        public static void ReportNotExhaustiveNullableEnumSwitch(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement) =>
            context.ReportDiagnostic(Diagnostic.Create(NotExhaustiveNullableEnumSwitch,
                switchStatement.SwitchKeyword.GetLocation()));

        public static void ReportNotExhaustiveNullableEnumSwitch(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchStatement) =>
            context.ReportDiagnostic(Diagnostic.Create(NotExhaustiveNullableEnumSwitch,
                switchStatement.SwitchKeyword.GetLocation()));

        private static readonly LocalizableString EM0001Title = LoadString(nameof(Resources.EM0001Title));
        private static readonly LocalizableString EM0001Message = LoadString(nameof(Resources.EM0001Message));
        private static readonly LocalizableString EM0001Description = LoadString(Resources.EM0001Description);
        private static readonly LocalizableString EM0002Title = LoadString(nameof(Resources.EM0002Title));
        private static readonly LocalizableString EM0002Message = LoadString(nameof(Resources.EM0002Message));
        private static readonly LocalizableString EM0002Description = LoadString(Resources.EM0002Description);
        private static readonly LocalizableString EM0003Title = LoadString(nameof(Resources.EM0003Title));
        private static readonly LocalizableString EM0003Message = LoadString(nameof(Resources.EM0003Message));
        private static readonly LocalizableString EM0003Description = LoadString(Resources.EM0003Description);
        private static readonly LocalizableString EM0011Title = LoadString(nameof(Resources.EM0011Title));
        private static readonly LocalizableString EM0011Message = LoadString(nameof(Resources.EM0011Message));
        private static readonly LocalizableString EM0011Description = LoadString(Resources.EM0011Description);
        private static readonly LocalizableString EM0012Title = LoadString(nameof(Resources.EM0012Title));
        private static readonly LocalizableString EM0012Message = LoadString(nameof(Resources.EM0012Message));
        private static readonly LocalizableString EM0012Description = LoadString(Resources.EM0012Description);
        private static readonly LocalizableString EM0013Title = LoadString(nameof(Resources.EM0013Title));
        private static readonly LocalizableString EM0013Message = LoadString(nameof(Resources.EM0013Message));
        private static readonly LocalizableString EM0013Description = LoadString(Resources.EM0013Description);
        private static readonly LocalizableString EM0014Title = LoadString(nameof(Resources.EM0014Title));
        private static readonly LocalizableString EM0014Message = LoadString(nameof(Resources.EM0014Message));
        private static readonly LocalizableString EM0014Description = LoadString(Resources.EM0014Description);
        private static readonly LocalizableString EM0015Title = LoadString(nameof(Resources.EM0015Title));
        private static readonly LocalizableString EM0015Message = LoadString(nameof(Resources.EM0015Message));
        private static readonly LocalizableString EM0015Description = LoadString(Resources.EM0015Description);
        private static readonly LocalizableString EM0100Title = LoadString(nameof(Resources.EM0100Title));
        private static readonly LocalizableString EM0100Message = LoadString(nameof(Resources.EM0100Message));
        private static readonly LocalizableString EM0100Description = LoadString(Resources.EM0100Description);
        private static readonly LocalizableString EM0101Title = LoadString(nameof(Resources.EM0101Title));
        private static readonly LocalizableString EM0101Message = LoadString(nameof(Resources.EM0101Message));
        private static readonly LocalizableString EM0101Description = LoadString(Resources.EM0101Description);
        private static readonly LocalizableString EM0102Title = LoadString(nameof(Resources.EM0102Title));
        private static readonly LocalizableString EM0102Message = LoadString(nameof(Resources.EM0102Message));
        private static readonly LocalizableString EM0102Description = LoadString(Resources.EM0102Description);
        private static readonly LocalizableString EM0103Title = LoadString(nameof(Resources.EM0103Title));
        private static readonly LocalizableString EM0103Message = LoadString(nameof(Resources.EM0103Message));
        private static readonly LocalizableString EM0103Description = LoadString(Resources.EM0103Description);
        private static readonly LocalizableString EM0104Title = LoadString(nameof(Resources.EM0104Title));
        private static readonly LocalizableString EM0104Message = LoadString(nameof(Resources.EM0104Message));
        private static readonly LocalizableString EM0104Description = LoadString(Resources.EM0104Description);
        private static readonly LocalizableString EM0105Title = LoadString(nameof(Resources.EM0105Title));
        private static readonly LocalizableString EM0105Message = LoadString(nameof(Resources.EM0105Message));
        private static readonly LocalizableString EM0105Description = LoadString(Resources.EM0105Description);

        private const string Category = "Logic";

        private static readonly DiagnosticDescriptor NotExhaustiveEnumSwitch =
            new DiagnosticDescriptor("EM0001", EM0001Title, EM0001Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0001Description);

        private static readonly DiagnosticDescriptor NotExhaustiveNullableEnumSwitch =
            new DiagnosticDescriptor("EM0002", EM0002Title, EM0002Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0002Description);

        public static readonly DiagnosticDescriptor NotExhaustiveObjectSwitch =
            new DiagnosticDescriptor("EM0003", EM0003Title, EM0003Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0003Description);

        public static readonly DiagnosticDescriptor ConcreteSubtypeMustBeCaseOfClosedType =
            new DiagnosticDescriptor("EM0011", EM0011Title, EM0011Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0011Description);

        public static readonly DiagnosticDescriptor MustBeDirectSubtype =
            new DiagnosticDescriptor("EM0012", EM0012Title, EM0012Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0012Description);

        public static readonly DiagnosticDescriptor MustBeSubtype =
            new DiagnosticDescriptor("EM0013", EM0013Title, EM0013Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0013Description);

        public static readonly DiagnosticDescriptor SubtypeMustBeCovered =
            new DiagnosticDescriptor("EM0014", EM0014Title, EM0014Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0014Description);

        public static readonly DiagnosticDescriptor OpenInterfaceSubtypeMustBeCaseOfClosedType =
            new DiagnosticDescriptor("EM0015", EM0015Title, EM0015Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0015Description);

        public static readonly DiagnosticDescriptor WhenGuardNotSupported =
            new DiagnosticDescriptor("EM0100", EM0100Title, EM0100Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0100Description);

        public static readonly DiagnosticDescriptor CasePatternNotSupported =
            new DiagnosticDescriptor("EM0101", EM0101Title, EM0101Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0101Description);

        public static readonly DiagnosticDescriptor OpenTypeNotSupported =
            new DiagnosticDescriptor("EM0102", EM0102Title, EM0102Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0102Description);

        public static readonly DiagnosticDescriptor MatchMustBeOnCaseType =
            new DiagnosticDescriptor("EM0103", EM0103Title, EM0103Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0103Description);

        public static readonly DiagnosticDescriptor DuplicateClosedAttribute =
            new DiagnosticDescriptor("EM0104", EM0104Title, EM0104Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0104Description);

        public static readonly DiagnosticDescriptor DuplicateCaseType =
            new DiagnosticDescriptor("EM0105", EM0105Title, EM0105Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0105Description);



        private static LocalizableResourceString LoadString(string name) =>
            new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
    }
}
