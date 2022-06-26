using System.ComponentModel;
using Microsoft.CodeAnalysis;

namespace ExhaustiveMatching.Analyzer.Enums.Semantics
{
    public static class TypeSymbolExtensions
    {
        /// <summary>
        /// Is this the <see cref="InvalidEnumArgumentException"/> type?
        /// </summary>
        /// <remarks>Checking this way avoids using <see cref="Compilation.GetTypeByMetadataName"/>
        /// which can return null if multiple types match a metadata name.</remarks>
        public static bool IsInvalidEnumArgumentException(this ITypeSymbol typeSymbol)
            => typeSymbol.MetadataName == typeof(InvalidEnumArgumentException).FullName;
    }
}
