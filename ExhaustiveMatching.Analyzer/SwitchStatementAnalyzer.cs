using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    public class SwitchStatementAnalyzer
    {
        public static void Analyze(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement)
        {
            var switchKind = IsExhaustive(context, switchStatement);
            if (!switchKind.IsExhaustive)
                return;

            var expressionTypeInfo = context.SemanticModel
                .GetTypeInfo(switchStatement.Expression, context.CancellationToken);

            if (expressionTypeInfo.Type?.TypeKind == TypeKind.Enum)
                AnalyzeSwitchOnEnum(context, switchStatement, expressionTypeInfo.Type);
            else if (!switchKind.DefaultThrowsInvalidEnum)
                AnalyzeSwitchOnClosed(context, switchStatement, expressionTypeInfo.Type);
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
                    var exhaustiveMatchFailedExceptionType = context.Compilation.GetTypeByMetadataName(TypeNames.ExhaustiveMatchFailedException);
                    var invalidEnumArgumentExceptionType = context.Compilation.GetTypeByMetadataName(TypeNames.InvalidEnumArgumentException);

                    var isExhaustiveMatchFailedException = exceptionType.Equals(exhaustiveMatchFailedExceptionType);
                    var isInvalidEnumArgumentException = exceptionType.Equals(invalidEnumArgumentExceptionType);
                    var isExhaustive = isExhaustiveMatchFailedException || isInvalidEnumArgumentException;

                    return new SwitchStatementKind(isExhaustive, isInvalidEnumArgumentException);
                }
            }

            return new SwitchStatementKind(false, false);
        }

        private static void AnalyzeSwitchOnEnum(
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
                .Select(m => m.Name)
                .ToArray();

            if (unusedSymbols.Any())
            {
                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.NotExhaustiveEnumSwitchRule,
                    switchStatement.GetLocation(),
                    string.Join(", ", unusedSymbols));
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeSwitchOnClosed(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            ITypeSymbol type)
        {
            // TODO check and report error for type does not have Closed attribute
            // `case var x when x is Square:` is a VarPattern

            var switchLabels = switchStatement
                .Sections.SelectMany(s => s.Labels).ToList();

            CheckForNonPatternCases(context, switchLabels);

            var typesUsed = switchLabels
                .OfType<CasePatternSwitchLabelSyntax>()
                .Select(casePattern => GetTypeSymbolMatched(context, casePattern))
                .ToImmutableHashSet();

            // GetTypeSymbolMatched returns null for fatal errors that prevent exhaustiveness checking
            if (typesUsed.Contains(default))
                return;

            var allTypes = GetAllConcreteClosedTypeMembers(context, type);

            var uncoveredTypes = allTypes.Where(t => !typesUsed.Any(t.IsSubtypeOf))
                .ToArray();

            if (uncoveredTypes.Any())
            {
                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.NotExhaustiveObjectSwitchRule,
                    switchStatement.GetLocation(), string.Join(", ", uncoveredTypes.Select(t => t.GetFullName())));
                context.ReportDiagnostic(diagnostic);
            }
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
                    var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.UnsupportedCaseClauseType,
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
                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.UnsupportedCaseClauseType,
                    casePattern.GetLocation(), casePattern.ToString());
                context.ReportDiagnostic(diagnostic);
                symbolUsed = null;
            }

            if (casePattern.WhenClause != null)
            {
                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.WhenClauseNotSupported,
                    casePattern.GetLocation(), casePattern.ToString());
                context.ReportDiagnostic(diagnostic);
            }

            return symbolUsed;
        }

        private static HashSet<ITypeSymbol> GetAllConcreteClosedTypeMembers(
            SyntaxNodeAnalysisContext context,
            ITypeSymbol type)
        {
            var closedAttribute = context.Compilation.GetTypeByMetadataName(TypeNames.ClosedAttribute);
            var concreteTypes = new HashSet<ITypeSymbol>();
            var types = new Queue<ITypeSymbol>();
            types.Enqueue(type);

            while (types.Count > 0)
            {
                type = types.Dequeue();
                if (!type.IsAbstract)
                    concreteTypes.Add(type);

                var unionOfTypes = type.UnionOfTypes(closedAttribute);

                foreach (var subtype in unionOfTypes)
                    types.Enqueue(subtype);
            }

            return concreteTypes;
        }
    }
}