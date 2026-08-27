using System.Runtime.InteropServices;

namespace Hanekawa.Extensions;

public static class ArrayExtensions
{
    public static T[] Remove<T>(this T[] collection, T entity)
    {
        var tempList = new List<T>(collection);
        tempList.Remove(entity);
        return tempList.ToArray();
    }

    public static T[] Add<T>(this T[] collection, T entity)
    {
        var tempList = new List<T>(collection) { entity };
        var span = CollectionsMarshal.AsSpan(tempList);
        return span.ToArray();
    }
}
