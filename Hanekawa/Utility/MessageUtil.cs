using System.Text.RegularExpressions;
using Disqord;
using Disqord.Gateway;
using Quartz.Util;

namespace Hanekawa.Utility
{
    public static class MessageUtil
    {
        private static Regex PlayerRegex => new Regex("%PLAYER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex UserRegex => new Regex("%USER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex ServerRegex => new Regex("%SERVER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex MembersRegex => new Regex("%MEMBERS%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static Regex UsernameRegex => new Regex("%USERNAME%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex MentionRegex => new Regex("%MENTION%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string FormatMessage(string msg, IMember user, CachedGuild guild)
        {
            if (msg.IsNullOrWhiteSpace()) return null;
            if (PlayerRegex.IsMatch(msg) && user != null) msg = PlayerRegex.Replace(msg, user.Mention);
            if (UserRegex.IsMatch(msg) && user != null) msg = UserRegex.Replace(msg, user.Mention);

            if (UsernameRegex.IsMatch(msg) && user != null) msg = UsernameRegex.Replace(msg, user.Nick);
            if (MentionRegex.IsMatch(msg) && user != null) msg = MentionRegex.Replace(msg, user.Mention);
            if (ServerRegex.IsMatch(msg) && guild != null) msg = ServerRegex.Replace(msg, guild.Name);
            if (MembersRegex.IsMatch(msg) && guild != null) msg = MembersRegex.Replace(msg, $"{guild.MaxMemberCount.Value + 1}");
            return msg;
        }
    }
}