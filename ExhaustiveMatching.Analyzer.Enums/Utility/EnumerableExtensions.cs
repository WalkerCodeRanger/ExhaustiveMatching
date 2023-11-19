using System.Collections.Generic;
using System.Linq;

namespace ExhaustiveMatching.Analyzer.Enums.Utility
{
    public static class EnumerableExtensions
    {
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> values) => values.ToList();

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> values) where T : class => values.Where(v => v != null)!;
    }
}
