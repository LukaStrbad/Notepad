using System;
using System.Collections.Generic;
using System.Linq;

namespace NotepadCore.ExtensionMethods
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TSource> Distinct<TSource, TResult>(this IEnumerable<TSource> enumerable,
            Func<TSource, TResult> selector)
        {
            var exists = new HashSet<TResult>(enumerable.Count());

            foreach (var element in enumerable)
            {
                if (exists.Add(selector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}