using System.Text.RegularExpressions;

namespace Jibril.Extensions
{
    public static class StringExtension
    {
        // Filter detection
        private static readonly Regex filterRegex =
            new Regex(@"(?:discord(?:\.gg|.me|app\.com\/invite)\/(?<id>([\w]{16}|(?:[\w]+-?){3})))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex scamFilterSteam = new Regex(
            @"(?:linkd\.in|bitly\.co|tcrn\.ch|bit\.ly|steam-community\.com|goo\.gl|tinyurl\.com|ow\.ly|strawpoli|steam-halloween\.com|google\.com|snip\.li|pointsprizes\.com|paysafecards\.org).*?(\s|$)",
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
            return scamFilterSteam.IsMatch(str);
        }
    }
}