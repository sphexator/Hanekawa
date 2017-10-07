using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Jibril.Extensions
{
    public static class StringExtension
    {
        private static readonly Regex filterRegex = new Regex(@"(?:discord(?:\.gg|.me|app\.com\/invite)\/(?<id>([\w]{16}|(?:[\w]+-?){3})))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static bool IsDiscordInvite(this string str)
            => filterRegex.IsMatch(str);
    }
}
