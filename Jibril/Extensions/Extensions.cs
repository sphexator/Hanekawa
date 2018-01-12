using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Extensions
{
    public static class Extensions
    {
        public static ConcurrentDictionary<TKey, TValue> ToConcurrent<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dict)
            => new ConcurrentDictionary<TKey, TValue>(dict);
    }
}
