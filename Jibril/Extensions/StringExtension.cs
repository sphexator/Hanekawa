using System.Text.RegularExpressions;

namespace Jibril.Extensions
{
    public static class StringExtension
    {
        // Filter detection
        private static readonly Regex filterRegex =
            new Regex(@"(?:discord(?:\.gg|.me|app\.com\/invite)\/(([\w]{16}|(?:[\w]+-?){3})))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex scamFilter = new Regex(
            @"(?:linkd\.in|bitly\.co|tcrn\.ch|bit\.ly|steam-community\.com|tinyurl\.com|ow\.ly|strawpoli|steam-halloween\.com|snip\.li|pointsprizes\.com|paysafecards\.org|c99\.nl|sentry\.mba|steamchristmas\.com).*?(\s|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex URLFilter =
            new Regex(@"/^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GoogleLink = 
            new Regex(@"(?:goo\.gl|google\.com).*?(\s|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Max Character string extension
        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        public static bool IsDiscordInvite(this string str)
        {
            return filterRegex.IsMatch(str);
        }

        public static bool IsScamLink(this string str)
        {
            return scamFilter.IsMatch(str);
        }

        public static bool IsGoogleLink(this string str)
        {
            return GoogleLink.IsMatch(str);
        }

        public static bool IsUrl(this string str)
        {
            return URLFilter.IsMatch(str);
        }
    }
}