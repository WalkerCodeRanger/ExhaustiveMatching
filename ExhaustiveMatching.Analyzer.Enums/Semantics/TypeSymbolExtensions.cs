using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums.Semantics
{
    public static class TypeSymbolExtensions
    {
        /// <summary>
        /// Is this the <see cref="InvalidEnumArgumentException"/> type?
        /// </summary>
        /// <remarks>Checking this way avoids using <see cref="Compilation.GetTypeByMetadataName"/>
        /// which can return <see langword="null"/> if multiple types match a metadata name.</remarks>
        public static bool IsInvalidEnumArgumentException(this ITypeSymbol typeSymbol)
            // TODO is there additional stuff that should be checked? (e.g. system assembly or correct base class)
            => typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
               == typeof(InvalidEnumArgumentException).FullName;

        public static bool IsEnum(
            this ITypeSymbol type,
            SyntaxNodeAnalysisContext context,
            out ITypeSymbol enumType,
            out bool nullable)
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
    }
}
