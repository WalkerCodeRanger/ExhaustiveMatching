﻿using System.Linq;
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
            {
                MustBeClosed(context, typeDeclaration, typeSymbol, closedAttribute);
                MustBeUnionMember(context, typeDeclaration, typeSymbol, closedAttribute);
            }

            if (IsClosedType(typeSymbol, closedAttribute))
                AllMemberTypesMustBeDirectSubtypes(context, typeDeclaration, typeSymbol, closedAttribute);
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
            var closedBaseTypes = typeSymbol.Interfaces.Append(typeSymbol.BaseType)
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

        private static void AllMemberTypesMustBeDirectSubtypes(
            SyntaxNodeAnalysisContext context,
            TypeDeclarationSyntax typeDeclaration,
            ITypeSymbol typeSymbol,
            INamedTypeSymbol closedAttribute)
        {
            var unionOfTypes = typeSymbol.UnionOfTypes(closedAttribute);
            foreach (var memberType in unionOfTypes)
            {
                if (memberType.BaseType.Equals(typeSymbol)
                || memberType.Interfaces.Any(i => i.Equals(typeSymbol)))
                    continue;

                var diagnostic = Diagnostic.Create(ExhaustiveMatchAnalyzer.MustBeDirectSubtype,
                    typeDeclaration.Identifier.GetLocation(), memberType.GetFullName());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}