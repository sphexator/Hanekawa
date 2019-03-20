using System.Text.RegularExpressions;

namespace Hanekawa.Extensions
{
    public static class RegexExtension
    {
        private static readonly Regex FilterRegex =
            new Regex(@"(?:discord(?:\.gg|.me|app\.com\/invite)\/(([\w]{16}|(?:[\w]+-?){3})))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ScamFilter =
            new Regex(
                @"(?:linkd\.in|bitly\.co|tcrn\.ch|bit\.ly|steam-community\.com|tinyurl\.com|ow\.ly|strawpoli|steam-halloween\.com|snip\.li|pointsprizes\.com|paysafecards\.org|c99\.nl|sentry\.mba|steamchristmas\.com).*?(\s|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GoogleLink =
            new Regex(@"(?:goo\.gl|google\.com).*?(\s|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PornLink =
            new Regex(@"(?:pornhub\.com).*?(\s|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex UrlRegex =
            new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex EmoteRegex =
            new Regex(@"(?:cdn\.discordapp\.com/emojis/).*?(\s|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex IpGrab =
            new Regex(@"(?:youramonkey\.com|robtex\.com).*?(\s|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsUrl(this string str) => !EmoteRegex.IsMatch(str) && UrlRegex.IsMatch(str);

        public static bool IsDiscordInvite(this string str) => FilterRegex.IsMatch(str);

        public static bool IsScamLink(this string str) => ScamFilter.IsMatch(str);

        public static bool IsGoogleLink(this string str) => GoogleLink.IsMatch(str);

        public static bool IsPornLink(this string str) => PornLink.IsMatch(str);

        public static bool IsIpGrab(this string str) => IpGrab.IsMatch(str);
    }
}