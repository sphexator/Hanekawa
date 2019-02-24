using System.Text;

namespace Hanekawa.Extensions
{
    public static class StringExtension
    {
        public static string SanitizeMentions(this string str) =>
            str.Replace("@everyone", "@everyοne").Replace("@here", "@һere");

        public static bool IsPictureUrl(this string str)
        {
            var isGif = str.EndsWith(".gif", true, null);
            var isPng = str.EndsWith(".png", true, null);
            var isJpeg = str.EndsWith(".jpeg", true, null);
            var isJpg = str.EndsWith(".jpg", true, null);

            return isGif || isPng || isJpg || isJpeg;
        }

        public static string FormatNumber(this uint num)
        {
            if (num >= 100000)
                return FormatNumber(num / 1000) + "K";
            if (num >= 10000) return (num / 1000D).ToString("0.#") + "K";
            return num.ToString("#,0");
        }

        public static string FormatNumber(this int num)
        {
            if (num >= 100000)
                return FormatNumber(num / 1000) + "K";
            if (num >= 10000) return (num / 1000D).ToString("0.#") + "K";
            return num.ToString("#,0");
        }
    }
}