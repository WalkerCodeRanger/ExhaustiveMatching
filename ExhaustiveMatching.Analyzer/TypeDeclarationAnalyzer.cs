using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    public class TypeDeclarationAnalyzer
    {
        private static readonly Type ClosedAttributeType = typeof(ClosedAttribute);

        public static void Analyze(
            SyntaxNodeAnalysisContext context,
            TypeDeclarationSyntax typeDeclaration)
        {
            var closedAttribute = context.Compilation.GetTypeByMetadataName(ClosedAttributeType.FullName);
            var typeSymbol = (ITypeSymbol)context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
            if (IsSubtypeOfClosedType(typeSymbol, closedAttribute))
            {
                MustBeClosed(context, typeDeclaration, typeSymbol, closedAttribute);
                MustBeUnionMember(context, typeDeclaration, typeSymbol, closedAttribute);
            }

            if (IsClosedType(typeSymbol, closedAttribute))
            {
                // All type members must be direct subtypes
            }
        }

        private static bool IsSubtypeOfClosedType(
            ITypeSymbol typeSymbol,
            INamedTypeSymbol closedAttribute)
        {
            return typeSymbol.InheritsFromTypeWithAttribute(closedAttribute);
        }

        private static void MustBeClosed(
            SyntaxNodeAnalysisContext context,
            TypeDeclarationSyntax typeDeclaration,
            ITypeSymbol typeSymbol,
            INamedTypeSymbol closedAttribute)
        {
            // must either be sealed/struct or union types
            if (typeSymbol.IsSealed
                || typeSymbol.IsValueType // i.e. `struct`
                || typeSymbol.HasAttribute(closedAttribute))
                return;

            var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.MustBeClosedType,
                typeDeclaration.Identifier.GetLocation(), typeSymbol.GetFullName());
            context.ReportDiagnostic(diagnostic);
        }

        private static void MustBeUnionMember(
            SyntaxNodeAnalysisContext context,
            TypeDeclarationSyntax typeDeclaration,
            ITypeSymbol typeSymbol,
            INamedTypeSymbol closedAttribute)
        {
            // Any type inheriting from a union type must be listed in the union
            var closedBaseTypes = typeSymbol.AllInterfaces.Append(typeSymbol.BaseType)
                .Where(t => t.HasAttribute(closedAttribute));

            foreach (var baseType in closedBaseTypes)
            {
                var isMember = baseType.UnionOfTypes(closedAttribute)
                                .Any(t => t.Equals(typeSymbol));
                if (isMember)
                    continue;

                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.MustBeUnionMember,
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
    }
}
