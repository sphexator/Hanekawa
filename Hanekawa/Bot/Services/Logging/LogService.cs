using Discord.WebSocket;
using Hanekawa.Core.Interfaces;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;

        public LogService(DiscordSocketClient client)
        {
            _client = client;

            _client.UserBanned += UserBanned;
            _client.UserUnbanned += UserUnbanned;

            _client.UserJoined += UserJoined;
            _client.UserLeft += UserLeft;

            _client.MessageDeleted += MessageDeleted;
            _client.MessageUpdated += MessageUpdated;

            _client.GuildMemberUpdated += GuildMemberUpdated;
            _client.UserUpdated += UserUpdated;
            
            _client.UserVoiceStateUpdated += VoiceLog;
        }
    }
}
