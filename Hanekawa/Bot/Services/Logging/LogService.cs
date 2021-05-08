using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hanekawa.Bot.Services.Caching;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly Logger _log;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;
        private readonly CacheService _cache;

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<string, Tuple<ulong, int>>> _invites = new();

        public LogService(Hanekawa client, IServiceProvider provider, ColourService colourService, CacheService cache)
        {
            _client = client;
            _log = LogManager.GetCurrentClassLogger();
            _provider = provider;
            _colourService = colourService;
            _cache = cache;

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
                var invites = _invites.GetOrAdd(e.GuildId.Value.RawValue, new ConcurrentDictionary<string, Tuple<ulong, int>>());
                invites.Remove(e.Code, out _);
                _invites.AddOrUpdate(e.GuildId.Value.RawValue, new ConcurrentDictionary<string, Tuple<ulong, int>>(),
                    (arg1, tuples) => invites);
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
                var invites = _invites.GetOrAdd(e.Guild.Id.RawValue, new ConcurrentDictionary<string, Tuple<ulong, int>>());
                invites.TryAdd(e.Code, new Tuple<ulong, int>(e.Inviter.Id.RawValue, 0));
                _invites.AddOrUpdate(e.Guild.Id.RawValue, new ConcurrentDictionary<string, Tuple<ulong, int>>(),
                    (arg1, tuples) => invites);
            });
            return Task.CompletedTask;
        }
    }
}