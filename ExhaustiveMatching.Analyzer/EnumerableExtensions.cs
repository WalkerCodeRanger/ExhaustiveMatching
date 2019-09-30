using System;
using System.Collections.Generic;

namespace ExhaustiveMatching.Analyzer
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SelectRecursive<T>(
            this IEnumerable<T> roots,
            Func<T, IEnumerable<T>> selector)
        {
            var stack = new Stack<T>();
            foreach (var root in roots)
                stack.Push(root);

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                yield return item;
                foreach (var child in selector(item))
                    stack.Push(child);
            }
        }
    }
}
