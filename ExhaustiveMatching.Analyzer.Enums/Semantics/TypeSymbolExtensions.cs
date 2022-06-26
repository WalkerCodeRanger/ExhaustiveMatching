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
        /// which can return <see langword="null"/> if multiple types match a metadata name.</remarks>
        public static bool IsInvalidEnumArgumentException(this ITypeSymbol typeSymbol)
            // TODO is there additional stuff that should be checked? (e.g. system assembly or correct base class)
            => typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
               == typeof(InvalidEnumArgumentException).FullName;
    }
}
