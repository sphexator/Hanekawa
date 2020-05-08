using System;
using Disqord;
using Disqord.Bot;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService : INService, IRequired
    {
        private readonly DiscordBot _client;
        private readonly InternalLogService _log;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;
        public LogService(DiscordBot client, InternalLogService log, IServiceProvider provider, ColourService colourService)
        {
            _client = client;
            _log = log;
            _provider = provider;
            _colourService = colourService;

            _client.MemberBanned += UserBanned;
            _client.MemberUnbanned += UserUnbanned;

            _client.MemberJoined += UserJoined;
            _client.MemberLeft += UserLeft;

            _client.MessageDeleted += MessageDeleted;
            _client.MessageUpdated += MessageUpdated;
            _client.MessagesBulkDeleted += MessagesBulkDeleted;

            _client.MemberUpdated += GuildMemberUpdated;
            _client.UserUpdated += UserUpdated;

            _client.VoiceStateUpdated += VoiceLog;
        }
    }
}