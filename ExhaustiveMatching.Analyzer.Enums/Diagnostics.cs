using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ExhaustiveMatching.Analyzer.Enums.Semantics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums
{
    internal static class Diagnostics
    {
        public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(NotExhaustiveEnumSwitch, NotExhaustiveNullableEnumSwitch);

        public static void ReportNotExhaustiveEnumSwitch(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            IEnumerable<ISymbol> unusedSymbols)
            => ReportNotExhaustiveEnumSwitch(context, switchStatement.SwitchKeyword, unusedSymbols);

        private static void ReportNotExhaustiveEnumSwitch(
            SyntaxNodeAnalysisContext context,
            SyntaxToken switchKeyword,
            IEnumerable<ISymbol> unusedSymbols)
        {
            var unusedSymbolNames = unusedSymbols.Select(s => s.ToErrorDisplayString());
            foreach (var symbolName in unusedSymbolNames.OrderBy(s => s))
            {
                var diagnostic = Diagnostic.Create(NotExhaustiveEnumSwitch, switchKeyword.GetLocation(), symbolName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        public static void ReportNotExhaustiveNullableEnumSwitch(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement) =>
            context.ReportDiagnostic(Diagnostic.Create(NotExhaustiveNullableEnumSwitch,
                switchStatement.SwitchKeyword.GetLocation()));

        private static readonly LocalizableString EM0001Title = LoadString(nameof(Resources.EM0001Title));
        private static readonly LocalizableString EM0001Message = LoadString(nameof(Resources.EM0001Message));
        private static readonly LocalizableString EM0001Description = LoadString(Resources.EM0001Description);
        private static readonly LocalizableString EM0002Title = LoadString(nameof(Resources.EM0002Title));
        private static readonly LocalizableString EM0002Message = LoadString(nameof(Resources.EM0002Message));
        private static readonly LocalizableString EM0002Description = LoadString(Resources.EM0002Description);

        private const string Category = "Logic";

        private static readonly DiagnosticDescriptor NotExhaustiveEnumSwitch
            = new DiagnosticDescriptor("EM0001", EM0001Title, EM0001Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0001Description);

        private static readonly DiagnosticDescriptor NotExhaustiveNullableEnumSwitch
            = new DiagnosticDescriptor("EM0002", EM0002Title, EM0002Message, Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, EM0002Description);

        private static LocalizableResourceString LoadString(string name)
            => new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
    }
}
