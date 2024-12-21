namespace Hanekawa.Extensions;

public static class NumberExtensions
{
    public static string Humanize(this int number)
        => number switch
        {
            < 1000 => number.ToString("G"),
            < 1000000 => $"{(number / 1000):G}K",
            < 1000000000 => $"{number / 1000000:G}M",
            _ => $"{number / 1000000000:G}B"
        };
}