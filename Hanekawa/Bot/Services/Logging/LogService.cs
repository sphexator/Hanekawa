using System;
using Discord.WebSocket;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly InternalLogService _log;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;
        public LogService(DiscordSocketClient client, InternalLogService log, IServiceProvider provider, ColourService colourService)
        {
            _client = client;
            _log = log;
            _provider = provider;
            _colourService = colourService;

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