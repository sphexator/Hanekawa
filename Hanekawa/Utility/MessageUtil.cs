using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Disqord;
using Quartz.Util;

namespace Hanekawa.Utility
{
    public static class MessageUtil
    {
        private static Regex PlayerRegex => new Regex("%PLAYER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex UserRegex => new Regex("%USER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex ServerRegex => new Regex("%SERVER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex MembersRegex => new Regex("%MEMBERS%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string FormatMessage(string msg, IMentionable user, CachedGuild guild)
        {
            if (msg.IsNullOrWhiteSpace()) return null;
            if (PlayerRegex.IsMatch(msg)) msg = PlayerRegex.Replace(msg, user.Mention);
            if (UserRegex.IsMatch(msg)) msg = UserRegex.Replace(msg, user.Mention);
            if (ServerRegex.IsMatch(msg)) msg = ServerRegex.Replace(msg, guild.Name);
            if (MembersRegex.IsMatch(msg)) msg = MembersRegex.Replace(msg, $"{guild.MemberCount + 1}");

            return msg;
        }
    }
}
