using System.Text;
using System.Text.RegularExpressions;

namespace Jibril.Extensions
{
    public static class StringExtension
    {
        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        

        // Filter detection
        private static readonly Regex FilterRegex =
            new Regex(@"(?:discord(?:\.gg|.me|app\.com\/invite)\/(([\w]{16}|(?:[\w]+-?){3})))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ScamFilter = new Regex(
            @"(?:linkd\.in|bitly\.co|tcrn\.ch|bit\.ly|steam-community\.com|tinyurl\.com|ow\.ly|strawpoli|steam-halloween\.com|snip\.li|pointsprizes\.com|paysafecards\.org|c99\.nl|sentry\.mba|steamchristmas\.com).*?(\s|$)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex UrlFilter =
            new Regex(@"/^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GoogleLink =
            new Regex(@"(?:goo\.gl|google\.com).*?(\s|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PornLink =
            new Regex(@"(?:pornhub\.com).*?(\s|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                private static readonly Regex IpGrab =
            new Regex(@"(?:youramonkey\.com|robtex\.com).*?(\s|$)", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Max Character string extension
        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        public static bool IsDiscordInvite(this string str)
        {
            return FilterRegex.IsMatch(str);
        }

        public static bool IsScamLink(this string str)
        {
            return ScamFilter.IsMatch(str);
        }

        public static bool IsGoogleLink(this string str)
        {
            return GoogleLink.IsMatch(str);
        }

        public static bool IsPornLink(this string str)
        {
            return PornLink.IsMatch(str);
        }

        public static bool IsUrl(this string str)
        {
            return UrlFilter.IsMatch(str);
        }
        public static bool IsIpGrab(this string str)
        {
            return IpGrab.IsMatch(str);
        }
    }
}