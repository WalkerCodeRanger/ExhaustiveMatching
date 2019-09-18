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

            var expressionType = context.SemanticModel
                .GetTypeInfo(switchStatement.Expression, context.CancellationToken)
                .Type;

            if (expressionType?.TypeKind == TypeKind.Enum)
                AnalyzeSwitchOnEnum(context, switchStatement, expressionType);
            else if (!switchKind.DefaultThrowsInvalidEnum)
                AnalyzeSwitchOnClosed(context, switchStatement, expressionType);
        }

        private static SwitchStatementKind IsExhaustive(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement)
        {
            var defaultSection = switchStatement.Sections
                .FirstOrDefault(s => s.Labels.OfType<DefaultSwitchLabelSyntax>().Any());

            var throwStatement = defaultSection?.Statements.OfType<ThrowStatementSyntax>().FirstOrDefault();

            // If there is no default section or it doesn't throw, we assume the
            // dev doesn't want an exhaustive match
            if (throwStatement != null)
            {
                var exceptionType = context.SemanticModel.GetTypeInfo(throwStatement.Expression, context.CancellationToken).Type;
                if (exceptionType != null && exceptionType.TypeKind != TypeKind.Error)
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

            foreach (var unusedSymbol in unusedSymbols.OrderBy(s => s))
            {
                var diagnostic = Diagnostic.Create(
                    ExhaustiveMatchAnalyzer.NotExhaustiveEnumSwitchRule,
                    switchStatement.SwitchKeyword.GetLocation(), unusedSymbol);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeSwitchOnClosed(
            SyntaxNodeAnalysisContext context,
            SwitchStatementSyntax switchStatement,
            ITypeSymbol type)
        {
            var switchLabels = switchStatement
                .Sections.SelectMany(s => s.Labels).ToList();

            CheckForNonPatternCases(context, switchLabels);

            var closedAttributeType = context.Compilation.GetTypeByMetadataName(TypeNames.ClosedAttribute);
            var isClosed = type.HasAttribute(closedAttributeType);

            var allCases = GetClosedTypeCases(type, closedAttributeType);

            var typesUsed = switchLabels
                .OfType<CasePatternSwitchLabelSyntax>()
                .Select(casePattern => GetTypeSymbolMatched(context, type, casePattern, allCases, isClosed))
                .Where(t => t != null) // returns null for invalid case clauses
                .ToImmutableHashSet();

            // If it is an open type, we don't want to actually check for uncovered types, but
            // we still needed to check the switch cases
            if (!isClosed)
            {
                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.OpenTypeNotSupported,
                    switchStatement.Expression.GetLocation(), type.GetFullName());
                context.ReportDiagnostic(diagnostic);
                return; // No point in trying to check for uncovered types, this isn't closed
            }

            var uncoveredTypes = allCases
                .Where(t => IsConcreteOrLeaf(t, closedAttributeType))
                .Where(t => !typesUsed.Any(t.IsSubtypeOf))
                .ToArray();

            foreach (var uncoveredType in uncoveredTypes.OrderBy(t => t.Name))
            {
                var diagnostic = Diagnostic.Create(
                    ExhaustiveMatchAnalyzer.NotExhaustiveObjectSwitchRule,
                    switchStatement.SwitchKeyword.GetLocation(), uncoveredType.GetFullName());
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
                    var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.CaseClauseTypeNotSupported,
                        switchLabel.Value.GetLocation(), switchLabel.Value.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static ITypeSymbol GetTypeSymbolMatched(
            SyntaxNodeAnalysisContext context,
            ITypeSymbol type,
            CasePatternSwitchLabelSyntax casePattern,
            HashSet<ITypeSymbol> allCases,
            bool isClosed)
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
                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.CaseClauseTypeNotSupported,
                    casePattern.Pattern.GetLocation(), casePattern.Pattern.ToString());
                context.ReportDiagnostic(diagnostic);
                symbolUsed = null;
            }

            if (isClosed && !allCases.Contains(symbolUsed))
            {
                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.MatchMustBeOnCaseType,
                    casePattern.Pattern.GetLocation(), symbolUsed.GetFullName(), type.GetFullName());
                context.ReportDiagnostic(diagnostic);
            }

            if (casePattern.WhenClause != null)
            {
                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.WhenGuardNotSupported,
                    casePattern.WhenClause.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }

            return symbolUsed;
        }

        private static HashSet<ITypeSymbol> GetClosedTypeCases(
            ITypeSymbol rootType,
            INamedTypeSymbol closedAttributeType)
        {
            var types = new HashSet<ITypeSymbol>();
            var queue = new Queue<ITypeSymbol>();
            queue.Enqueue(rootType);

            while (queue.Count > 0)
            {
                var caseType = queue.Dequeue();

                // Skip over errors or things that aren't subtypes at all
                if (rootType.TypeKind == TypeKind.Error
                    || !caseType.IsSubtypeOf(rootType))
                    continue;

                types.Add(caseType);

                var caseTypes = caseType.GetValidCaseTypes(closedAttributeType);

                foreach (var subtype in caseTypes)
                    // don't process a type more than once to avoid cycles
                    if (!types.Contains(subtype))
                        queue.Enqueue(subtype);
            }

            return types;
        }

        private static bool IsConcreteOrLeaf(ITypeSymbol type, INamedTypeSymbol closedAttributeType)
        {
            return type != null
                     && type.TypeKind != TypeKind.Error
                     && (!type.IsAbstract
                        || !type.HasAttribute(closedAttributeType));
        }
    }
}