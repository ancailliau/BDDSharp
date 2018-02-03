using System.Collections.Generic;

namespace UCLouvain.BDDSharp
{
    static class IEnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}