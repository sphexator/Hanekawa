using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Hanekawa.Entities.Interfaces;
using Quartz.Util;

namespace Hanekawa.Services.Welcome
{
    public class WelcomeMessage : IHanaService
    {
        private static Regex PlayerRegex => new Regex("%PLAYER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex UserRegex => new Regex("%USER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex ServerRegex => new Regex("%SERVER%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex MembersRegex => new Regex("%MEMBERS%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string Message(string msg, SocketGuildUser user) => CreateMessage(msg, user, user.Guild);
        public string Message(string msg, IGuildUser user, SocketGuild guild) => CreateMessage(msg, user, guild);
        public string Message(string msg, IUser user, SocketGuild guild) => CreateMessage(msg, user, guild);

        private static string CreateMessage(string msg, IMentionable user, SocketGuild guild)
        {
            if (msg.IsNullOrWhiteSpace()) return null;
            if (PlayerRegex.IsMatch(msg) || UserRegex.IsMatch(msg)) msg = PlayerRegex.Replace(msg, user.Mention);
            if (ServerRegex.IsMatch(msg)) msg = ServerRegex.Replace(msg, guild.Name);
            if (MembersRegex.IsMatch(msg)) msg = MembersRegex.Replace(msg, $"{guild.MemberCount + 1}");

            return msg;
        }
    }
}