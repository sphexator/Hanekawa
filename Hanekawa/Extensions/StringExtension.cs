using System.Linq;
using System.Text.RegularExpressions;

namespace Hanekawa.Extensions
{
    public static class StringExtension
    {
        public static string SanitizeEveryone(this string str) =>
            str.Replace("@everyone", "@everyοne").Replace("@here", "@һere");
        
        private static readonly Regex FilterRegex =
            new Regex(@"(?:discord(?:\.gg|.me|app\.com\/invite)\/(([\w]{16}|(?:[\w]+-?){3})))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsPictureUrl(this string str)
        {
            var isGif = str.EndsWith(".gif");
            var isPng = str.EndsWith(".png");
            var isJpeg = str.EndsWith(".jpeg");
            var isJpg = str.EndsWith(".jpg");

            if (isGif) return true;
            if (isPng) return true;
            if (isJpeg) return true;
            if (isJpg) return true;
            return false;
        }

        public static bool IsDiscordInvite(this string str, out string invite)
        {
            if (FilterRegex.IsMatch(str))
            {
                var invites = FilterRegex.GetGroupNames().FirstOrDefault();
                invite = invites;
                return true;
            }

            invite = null;
            return false;
        }
    }
}