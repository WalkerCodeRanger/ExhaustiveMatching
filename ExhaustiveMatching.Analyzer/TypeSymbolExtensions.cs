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

        public static bool IsDirectSubtypeOf(this ITypeSymbol symbol, ITypeSymbol type)
        {
            return symbol.DirectlyImplements(type) || Equals(symbol.BaseType, type);
        }

        public static bool DirectlyImplements(this ITypeSymbol symbol, ITypeSymbol type)
        {
            return symbol.Interfaces.Any(type.Equals);
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

        public static bool IsDirectSubtypeOfTypeWithAttribute(
            this ITypeSymbol symbol,
            INamedTypeSymbol attributeType)
        {
            return symbol.Interfaces.Any(i => i.HasAttribute(attributeType))
                   || (symbol.BaseType?.HasAttribute(attributeType) ?? false);
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
                .SelectMany(GetTypeConstants)
                .Select(arg => arg.Value)
                .Cast<ITypeSymbol>();
        }

        public static IEnumerable<ITypeSymbol> GetValidCaseTypes(
            this ITypeSymbol type,
            INamedTypeSymbol closedAttributeType)
        {
            return type.GetCaseTypes(closedAttributeType)
                .Where(t => t.TypeKind != TypeKind.Error && t.IsDirectSubtypeOf(type));
        }

        public static IEnumerable<ITypeSymbol> GetLeafCaseTypes(
            this ITypeSymbol type,
            INamedTypeSymbol closedAttributeType)
        {
            return type.GetCaseTypes(closedAttributeType)
                .Where(t => !t.HasAttribute(closedAttributeType));
        }

        private static IEnumerable<TypedConstant> GetTypeConstants(TypedConstant constant)
        {
            // Ignore anything that isn't a type or type in a single array. The compiler will report
            // them as type errors.
            switch (constant.Kind)
            {
                case TypedConstantKind.Type:
                    yield return constant;
                    break;
                case TypedConstantKind.Array:
                    foreach (var constantValue in constant.Values)
                        if (constantValue.Kind == TypedConstantKind.Type)
                            yield return constantValue;
                    break;
            }
        }

        public static string GetFullName(this ISymbol symbol)
        {
            var ns = symbol.ContainingNamespace;
            return ns != null && !ns.IsGlobalNamespace ? $"{ns.GetFullName()}.{symbol.Name}" : symbol.Name;
        }

        public static IEnumerable<ITypeSymbol> DirectSuperTypes(this ITypeSymbol type)
        {
            IEnumerable<ITypeSymbol> baseTypes = type.Interfaces;
            if (type.BaseType != null)
                baseTypes = baseTypes.Append(type.BaseType);
            return baseTypes;
        }

        public static IEnumerable<ITypeSymbol> AllSuperTypes(this ITypeSymbol type)
        {
            return type.AllInterfaces.Concat(type.BaseClasses());
        }
    }
}
