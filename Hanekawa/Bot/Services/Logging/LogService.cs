using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public LogService(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;

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
