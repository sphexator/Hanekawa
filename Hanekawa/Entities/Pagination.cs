namespace Hanekawa.Entities;

public class Pagination<T>(T[] items)
{
    public T[] Items { get; set; } = items;
    public int Size { get; set; } = items.Length;
}