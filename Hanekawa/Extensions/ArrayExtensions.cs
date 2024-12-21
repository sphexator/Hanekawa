using System.Runtime.InteropServices;

namespace Hanekawa.Extensions;

public static class ArrayExtensions
{
    public static void Remove<T>(this T[] collection, T entity)
    {
        var tempList = new List<T>(collection);
        tempList.Remove(entity);
        var span = CollectionsMarshal.AsSpan(tempList);
        collection = span.ToArray();
    }

    public static void Add<T>(this T[] collection, T entity)
    {
        var tempList = new List<T>(collection) { entity };
        var span = CollectionsMarshal.AsSpan(tempList);
        collection = span.ToArray();
    }
}