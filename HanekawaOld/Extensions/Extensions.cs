using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Hanekawa.Extensions
{
    public static class Extensions
    {
        public static ConcurrentDictionary<TKey, TValue> ToConcurrent<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> dict) =>
            new ConcurrentDictionary<TKey, TValue>(dict);
    }
}