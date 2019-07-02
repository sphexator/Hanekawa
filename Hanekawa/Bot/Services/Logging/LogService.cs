using Discord.WebSocket;
using Hanekawa.Shared.Interfaces;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly InternalLogService _log;

        public LogService(DiscordSocketClient client, InternalLogService log)
        {
            _client = client;
            _log = log;

            _client.UserBanned += UserBanned;
            _client.UserUnbanned += UserUnbanned;

            _client.UserJoined += UserJoined;
            _client.UserLeft += UserLeft;

            _client.MessageDeleted += MessageDeleted;
            _client.MessageUpdated += MessageUpdated;
            _client.MessagesBulkDeleted += MessagesBulkDeleted;

            _client.GuildMemberUpdated += GuildMemberUpdated;
            _client.UserUpdated += UserUpdated;
            
            _client.UserVoiceStateUpdated += VoiceLog;
        }
    }
}
