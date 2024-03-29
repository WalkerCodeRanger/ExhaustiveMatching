using System.Collections.Generic;
using System.Linq;

namespace ExhaustiveMatching.Analyzer.Enums.Utility
{
    public static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> values)
            => new HashSet<T>(values);

        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> values)
            => values.ToList().AsReadOnly();

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> values)
            where T : class
            => values.Where(v => v != null);
    }
}
