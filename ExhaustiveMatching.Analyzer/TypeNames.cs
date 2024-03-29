using System.ComponentModel;

namespace ExhaustiveMatching.Analyzer
{
    /// <summary>
    /// Full names for types the analyzer uses the metadata for.
    /// </summary>
    /// <remarks>Types in the ExhaustiveMatching assembly are not referenced to prevent needing to
    /// distribute that assembly as part of the analyzer in addition to the actual dependencies.</remarks>
    internal static class TypeNames
    {
        public const string ExhaustiveMatchFailedException = "ExhaustiveMatching.ExhaustiveMatchFailedException";
        public const string ClosedAttribute = "ExhaustiveMatching.ClosedAttribute";
        public static readonly string InvalidEnumArgumentException = typeof(InvalidEnumArgumentException).FullName;
        public static readonly string Nullable = typeof(System.Nullable<>).FullName;
    }
}
