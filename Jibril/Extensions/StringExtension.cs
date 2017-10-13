using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Jibril.Extensions
{
    public static class StringExtension
    {
        private static readonly Regex filterRegex = new Regex(@"(?:discord(?:\.gg|.me|app\.com\/invite)\/(?<id>([\w]{16}|(?:[\w]+-?){3})))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex scamFilterSteam = new Regex(@"(?:linkd\.in|t\.co|bitly\.co|tcrn\.ch|bit\.ly|steam-community\.com|goo\.gl|tinyurl\.com|ow\.ly).*?(\s|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static bool IsDiscordInvite(this string str)
            => filterRegex.IsMatch(str);
        public static bool IsScamLink(this string str)
            => scamFilterSteam.IsMatch(str);
    }
}
