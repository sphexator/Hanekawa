using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Quartz.Util;

namespace Hanekawa.Bot.Services.Welcome
{
    public partial class WelcomeService
    {
        private Regex PlayerRegex => new Regex("%PLAYER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private Regex UserRegex => new Regex("%USER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private Regex ServerRegex => new Regex("%SERVER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private Regex MembersRegex => new Regex("%MEMBERS%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private string CreateMessage(string msg, IMentionable user, SocketGuild guild)
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