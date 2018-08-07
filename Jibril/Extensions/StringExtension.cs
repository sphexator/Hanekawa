using System.Text;

namespace Hanekawa.Extensions
{
    public static class StringExtension
    {
        public static string RemoveSpecialCharacters(this string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if ((c >= '0' && c <= '9') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z') ||
                    c == '.' ||
                    c == '_' ||
                    c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string SanitizeMentions(this string str) =>
            str.Replace("@everyone", "@everyοne").Replace("@here", "@һere");

    }
}