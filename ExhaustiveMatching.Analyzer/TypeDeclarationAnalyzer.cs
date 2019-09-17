using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    public class TypeDeclarationAnalyzer
    {
        public static void Analyze(
            SyntaxNodeAnalysisContext context,
            TypeDeclarationSyntax typeDeclaration)
        {
            var closedAttribute = context.Compilation.GetTypeByMetadataName(TypeNames.ClosedAttribute);
            var typeSymbol = (ITypeSymbol)context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
            if (IsSubtypeOfClosedType(typeSymbol, closedAttribute))
                MustBeCase(context, typeDeclaration, typeSymbol, closedAttribute);

            if (IsClosedType(typeSymbol, closedAttribute))
                AllMemberTypesMustBeDirectSubtypes(context, typeDeclaration, typeSymbol, closedAttribute);
        }

        private static bool IsSubtypeOfClosedType(
            ITypeSymbol typeSymbol,
            INamedTypeSymbol closedAttribute)
        {
            return typeSymbol.InheritsFromTypeWithAttribute(closedAttribute);
        }

        private static void MustBeCase(
            SyntaxNodeAnalysisContext context,
            TypeDeclarationSyntax typeDeclaration,
            ITypeSymbol typeSymbol,
            INamedTypeSymbol closedAttribute)
        {
            // Any type inheriting from a closed type must be listed in the cases
            IEnumerable<ITypeSymbol> baseTypes = typeSymbol.Interfaces;
            if (typeSymbol.BaseType != null)
                baseTypes = baseTypes.Append(typeSymbol.BaseType);
            var closedBaseTypes = baseTypes
                .Where(t => t.HasAttribute(closedAttribute));

            foreach (var baseType in closedBaseTypes)
            {
                var isMember = baseType.GetCaseTypes(closedAttribute)
                                .Any(t => t.Equals(typeSymbol));
                if (isMember)
                    continue;

                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.MustBeCaseOfClosedType,
                    typeDeclaration.Identifier.GetLocation(), typeSymbol.GetFullName(), baseType.GetFullName());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsClosedType(
            ITypeSymbol typeSymbol,
            INamedTypeSymbol closedAttribute)
        {
            return typeSymbol.HasAttribute(closedAttribute);
        }

        private static void AllMemberTypesMustBeDirectSubtypes(
            SyntaxNodeAnalysisContext context,
            TypeDeclarationSyntax typeDeclaration,
            ITypeSymbol typeSymbol,
            INamedTypeSymbol closedAttribute)
        {
            var caseTypeSyntaxes = typeSymbol.GetCaseTypeSyntaxes(closedAttribute);
            foreach (var caseTypeSyntax in caseTypeSyntaxes)
            {
                var caseType = context.SemanticModel.GetTypeInfo(caseTypeSyntax).Type;
                if (caseType == null
                    || typeSymbol.Equals(caseType.BaseType) // BaseType is null for interfaces, avoid calling method on it
                    || caseType.Interfaces.Any(i => i.Equals(typeSymbol)))
                    continue;

                if (caseType.InheritsFrom(typeSymbol)
                    || caseType.AllInterfaces.Any(i => i.Equals(typeSymbol)))
                {
                    // It's a subtype, just not a direct one
                    var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.MustBeDirectSubtype,
                        caseTypeSyntax.GetLocation(), caseType.GetFullName());
                    context.ReportDiagnostic(diagnostic);
                }
                else
                {
                    // Not even a subtype
                    var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.MustBeSubtype,
                        caseTypeSyntax.GetLocation(), caseType.GetFullName());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
