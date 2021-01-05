using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly InternalLogService _log;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;

        private readonly ConcurrentDictionary<ulong, HashSet<Tuple<string, ulong, int>>> _invites =
            new ConcurrentDictionary<ulong, HashSet<Tuple<string, ulong, int>>>();

        public LogService(Hanekawa client, InternalLogService log, IServiceProvider provider, ColourService colourService)
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

            _client.ReactionAdded += ReactionAddLog;
            _client.ReactionRemoved += ReactionRemovedLog;

            _client.MemberUpdated += GuildMemberUpdated;
            _client.UserUpdated += UserUpdated;

            _client.VoiceStateUpdated += VoiceLog;
            _client.InviteCreated += InviteCreated;
            _client.InviteDeleted += InviteDeleted;
        }

        private Task InviteDeleted(Disqord.Events.InviteDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!e.GuildId.HasValue) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(e.GuildId.Value.RawValue);
                if (!cfg.LogJoin.HasValue) return;
                var invites = _invites.GetOrAdd(e.GuildId.Value.RawValue, new HashSet<Tuple<string, ulong, int>>());
                invites.RemoveWhere(x => x.Item1 == e.Code);
            });
            return Task.CompletedTask;
        }

        private Task InviteCreated(Disqord.Events.InviteCreatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(e.Guild);
                if (!cfg.LogJoin.HasValue) return;
                var invites = _invites.GetOrAdd(e.Guild.Id.RawValue, new HashSet<Tuple<string, ulong, int>>());
                invites.Add(new Tuple<string, ulong, int>(e.Code, e.Inviter.Id.RawValue, 0));
            });
            return Task.CompletedTask;
        }
    }
}