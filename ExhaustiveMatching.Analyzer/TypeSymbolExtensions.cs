using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExhaustiveMatching.Analyzer
{
    internal static class TypeSymbolExtensions
    {
        public static bool IsSubtypeOf(this ITypeSymbol symbol, ITypeSymbol type)
        {
            return symbol.Equals(type) || symbol.Implements(type) || symbol.InheritsFrom(type);
        }

        public static bool Implements(this ITypeSymbol symbol, ITypeSymbol type)
        {
            return symbol.AllInterfaces.Any(type.Equals);
        }

        public static IEnumerable<INamedTypeSymbol> BaseClasses(this ITypeSymbol symbol)
        {
            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        public static bool InheritsFrom(this ITypeSymbol symbol, ITypeSymbol type)
        {
            return symbol.BaseClasses().Any(t => t.Equals(type));
        }

        public static bool InheritsFromTypeWithAttribute(
            this ITypeSymbol symbol,
            INamedTypeSymbol attributeType)
        {
            return symbol.AllInterfaces.Any(i => i.HasAttribute(attributeType))
                   || symbol.BaseClasses().Any(t => t.HasAttribute(attributeType));
        }

        public static bool HasAttribute(this ITypeSymbol symbol, INamedTypeSymbol attributeType)
        {
            return symbol.GetAttributes().Any(a => a.AttributeClass.Equals(attributeType));
        }

        public static IEnumerable<TypeSyntax> GetCaseTypeSyntaxes(
            this ITypeSymbol type,
            INamedTypeSymbol closedAttributeType)
        {
            return type.GetAttributes()
                .Where(attr => attr.AttributeClass.Equals(closedAttributeType))
                .Select(attr => attr.ApplicationSyntaxReference.GetSyntax()).Cast<AttributeSyntax>()
                .SelectMany(attr => attr.ArgumentList.Arguments)
                .Select(arg => arg.Expression)
                .OfType<TypeOfExpressionSyntax>()
                .Select(s => s.Type);
        }

        public static IEnumerable<ITypeSymbol> GetCaseTypes(
            this ITypeSymbol type,
            INamedTypeSymbol closedAttributeType)
        {
            return type.GetAttributes()
                .Where(a => a.AttributeClass.Equals(closedAttributeType))
                .SelectMany(a => a.ConstructorArguments)
                .SelectMany(arg => arg.Values)
                .Select(arg => arg.Value)
                .Cast<ITypeSymbol>();
        }

        public static string GetFullName(this ISymbol symbol)
        {
            var ns = symbol.ContainingNamespace;
            return ns != null && !ns.IsGlobalNamespace ? $"{ns.GetFullName()}.{symbol.Name}" : symbol.Name;
        }
    }
}
