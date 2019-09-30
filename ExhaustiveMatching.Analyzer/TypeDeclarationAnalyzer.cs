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
            if (typeSymbol.IsDirectSubtypeOfTypeWithAttribute(closedAttribute))
                MustBeCase(context, typeDeclaration, typeSymbol, closedAttribute);

            if (IsClosedType(typeSymbol, closedAttribute))
                AllMemberTypesMustBeDirectSubtypes(context, typeSymbol, closedAttribute);
        }

        private static void MustBeCase(
            SyntaxNodeAnalysisContext context,
            TypeDeclarationSyntax typeDeclaration,
            ITypeSymbol typeSymbol,
            INamedTypeSymbol closedAttribute)
        {
            if (typeSymbol.IsAbstract)
                return;

            // Any concrete type directly inheriting from a closed type must be listed in the cases
            var directSuperTypes = typeSymbol.DirectSuperTypes();
            var closedSuperTypes = directSuperTypes
                .Where(t => t.HasAttribute(closedAttribute))
                .ToList();

            foreach (var superType in closedSuperTypes)
            {
                var isMember = superType.GetCaseTypes(closedAttribute)
                                .Any(t => t.Equals(typeSymbol));
                if (isMember)
                    continue;

                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.ConcreteSubtypeMustBeCaseOfClosedType,
                    typeDeclaration.Identifier.GetLocation(), typeSymbol.GetFullName(), superType.GetFullName());
                context.ReportDiagnostic(diagnostic);
            }

            // Any concrete type indirectly inheriting from a closed type must be covered by a case type
            // that isn't itself closed. If it were closed, then it could be matched by all the cases, and
            // this type would not be matched.
            var otherClosedSuperTypes = typeSymbol.AllSuperTypes()
                .Where(t => t.HasAttribute(closedAttribute))
                .Except(closedSuperTypes);

            foreach (var superType in otherClosedSuperTypes)
            {
                var isCovered = superType
                    .GetLeafCaseTypes(closedAttribute)
                    .Any(typeSymbol.IsSubtypeOf);

                if (isCovered)
                    continue;

                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.SubtypeMustBeCovered,
                    typeDeclaration.Identifier.GetLocation(), typeSymbol.GetFullName(), superType.GetFullName());
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
