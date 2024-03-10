using System.Runtime.InteropServices;
using System.Text;
using Hanekawa.Entities;

namespace Hanekawa.Extensions;

public static class PaginationBuilderExtension
{
    public static string[] BuildPage<T>(this IList<T> list) 
    {
        var pages = new List<string>(list.Count / 5 + 1);
        for (var i = 0; i < list.Count;)
        {
            var sb = new StringBuilder();
            for (var j = 0; j < 5; j++)
            {
                if(i >= list.Count) break;
                var x = list[i];
                if(x is null) continue;
                sb.AppendLine(x.ToString());
                i++;
            }
            pages.Add(sb.ToString());
        }
        
        return CollectionsMarshal.AsSpan(pages).ToArray();
    }
    
    public static string[] BuildPage<T>(this T[] list)
    {
        var pages = new List<string>(list.Length / 5 + 1);
        for (var i = 0; i < list.Length;)
        {
            var sb = new StringBuilder();
            for (var j = 0; j < 5; j++)
            {
                if(i >= list.Length) break;
                var x = list[i];
                if(x is null) continue;
                sb.AppendLine(x.ToString());
                i++;
            }
            pages.Add(sb.ToString());
        }
        return CollectionsMarshal.AsSpan(pages).ToArray();
    }
    
    public static T[] Paginate<T> (this string[] list) where T : Message, new()
    {
        var pages = new T[list.Length / 5 + 1];
        for (var i = 0; i < list.Length; i++)
        {
            var x = list[i];
            pages[i] = new() { Content = x };
        }
        return pages;
    }
}