using System.Text.RegularExpressions;

namespace Hanekawa.Addons.AnimeSimulCast.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex Season = new Regex(@"(S([1-9]{1,4}))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Episode = new Regex(@"( - Episode ([\w]{1,4}))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        public static string GetSeason(this string content)
        {
            return !Season.IsMatch(content) ? null : content.Remove(0, 1);
        }

        public static string GetEpisode(this string content)
        {
            var fullString = Episode.Match(content).Value;
            return fullString.Remove(0, 11);
        }

        public static string Filter(this string content)
        {
            var filt = Episode.Replace(content, "");
            return Season.IsMatch(filt) ? Season.Replace(filt, "") : filt;
        }
    }
}
