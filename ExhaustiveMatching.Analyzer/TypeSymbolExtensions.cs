using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

        /// <summary>
        /// Get the case types of a type. (one level, not recursive)
        /// </summary>
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
            return new[] { type }
                .SelectRecursive(t => t.GetCaseTypes(closedAttributeType))
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

        public static bool IsEnum(this ITypeSymbol type, SyntaxNodeAnalysisContext context, out ITypeSymbol enumType, out bool nullable)
        {
            switch (type.TypeKind)
            {
                case TypeKind.Enum:
                    enumType = type;
                    nullable = false;
                    return true;
                case TypeKind.Struct:
                    var nullableType = context.Compilation.GetTypeByMetadataName(TypeNames.Nullable);
                    if (type.OriginalDefinition.Equals(nullableType))
                    {
                        type = ((INamedTypeSymbol)type).TypeArguments.Single();
                        if (type.TypeKind == TypeKind.Enum)
                        {
                            enumType = type;
                            nullable = true;
                            return true;
                        }
                    }
                    break;
            }

            enumType = null;
            nullable = false;
            return false;
        }

        public static HashSet<ITypeSymbol> GetClosedTypeCases(
            this ITypeSymbol rootType,
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

        public static bool IsConcreteOrLeaf(this ITypeSymbol type, INamedTypeSymbol closedAttributeType)
        {
            return type != null
                   && type.TypeKind != TypeKind.Error
                   && (!type.IsAbstract
                       || !type.HasAttribute(closedAttributeType));
        }
    }
}
