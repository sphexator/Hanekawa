using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Giveaway;
using Hanekawa.Shared;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Account = Hanekawa.Database.Tables.Account.Account;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly Logger _log;
        private readonly Random _random;
        private readonly IServiceProvider _provider;

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerCategoryReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerTextChanReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerVoiceChanReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public ExpService(Hanekawa client, Random random, InternalLogService log, IServiceProvider provider)
        {
            _client = client;
            _random = random;
            _log = LogManager.GetCurrentClassLogger();
            _provider = provider;

            _ = EventHandler(new CancellationToken());

            _client.MessageReceived += ServerMessageExpAsync;
            _client.MessageReceived += GlobalMessageExpAsync;
            _client.VoiceStateUpdated += VoiceExpAsync;
            _client.MemberJoined += GiveRolesBackAsync;
            _client.RoleDeleted += RemoveRole;

            using var scope = _provider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            foreach (var x in db.LevelConfigs)
            {
                _textExpMultiplier.TryAdd(x.GuildId, x.TextExpMultiplier);
                _voiceExpMultiplier.TryAdd(x.GuildId, x.VoiceExpMultiplier);
            }

            foreach (var x in db.LevelExpReductions)
            {
                try
                {
                    switch (x.ChannelType)
                    {
                        case ChannelType.Category:
                        {
                            var categories = ServerCategoryReduction.GetOrAdd(x.GuildId, new HashSet<ulong>());
                            categories.Add(x.ChannelId);
                            ServerCategoryReduction.AddOrUpdate(x.GuildId, new HashSet<ulong>(),
                                (arg1, list) => categories);
                            break;
                        }

                        case ChannelType.Text:
                        {
                            var channel = ServerTextChanReduction.GetOrAdd(x.GuildId, new HashSet<ulong>());
                            channel.Add(x.ChannelId);
                            ServerTextChanReduction.AddOrUpdate(x.GuildId, new HashSet<ulong>(),
                                (arg1, list) => channel);
                            break;
                        }

                        case ChannelType.Voice:
                        {
                            var channel = ServerVoiceChanReduction.GetOrAdd(x.GuildId, new HashSet<ulong>());
                            channel.Add(x.ChannelId);
                            ServerVoiceChanReduction.AddOrUpdate(x.GuildId, new HashSet<ulong>(),
                                (arg1, list) => channel);
                            break;
                        }
                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    _log.Log(NLog.LogLevel.Error, e, $"Couldn't load {x.GuildId} reward plugin for {x.ChannelId}, remove?");
                }
            }
        }

        private Task GlobalMessageExpAsync(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!(e.Message.Author is CachedMember user)) return;
                if (!(e.Message.Channel is CachedTextChannel channel)) return;
                if (user.IsBot) return;
                if (OnGlobalCooldown(user)) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var userData = await db.GetOrCreateGlobalUserData(user);
                    await AddExpAsync(userData, GetExp(channel), _random.Next(1, 3), db);
                }
                catch (Exception e)
                {
                    _log.Log(NLog.LogLevel.Error, e,
                        $"(Exp Service) Error in {user.Guild.Id.RawValue} for Global Exp - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task ServerMessageExpAsync(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!(e.Message.Author is CachedMember user)) return;
                if (!(e.Message.Channel is CachedTextChannel channel)) return;
                if (user.IsBot) return;
                if (OnServerCooldown(user)) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var userData = await db.GetOrCreateUserData(user);
                    userData.LastMessage = DateTime.UtcNow;
                    if(!userData.FirstMessage.HasValue) userData.FirstMessage = DateTime.UtcNow;
                    await AddExpAsync(user, userData, GetExp(channel), _random.Next(0, 3), db);
                    await MvpCount(db, userData, user);
                    await GiveawayAsync(db, user);
                }
                catch (Exception z)
                {
                    _log.Log(NLog.LogLevel.Error, z,
                        $"(Exp Service) Error in {user.Guild.Id.RawValue} for Server Exp - {z.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task VoiceExpAsync(VoiceStateUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.Member;
                if (user.IsBot) return;
                var after = e.NewVoiceState;
                var before = e.OldVoiceState;
                if (user.IsBot) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);
                    if (!cfg.VoiceExpEnabled) return;
                    if (before != null && after != null) return;
                    var userData = await db.GetOrCreateUserData(user);
                    if (before == null && after != null)
                    {
                        userData.VoiceExpTime = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        return;
                    }

                    if (before != null && after == null)
                    {
                        user.Guild.VoiceChannels.TryGetValue(before.ChannelId, out var vcChannel);
                        var exp = GetExp(vcChannel, DateTime.UtcNow - userData.VoiceExpTime);
                        await AddExpAsync(user, userData, exp, Convert.ToInt32(exp / 2), db);
                    }
                }
                catch (Exception z)
                {
                    _log.Log(NLog.LogLevel.Error, z,
                        $"(Exp Service) Error in {user.Guild.Id.RawValue} for Voice - {z.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private int GetExp(CachedTextChannel channel)
        {
            var xp = _random.Next(10, 20);
            if (IsReducedExp(channel)) xp = Convert.ToInt32(xp / 10);
            return xp;
        }

        private int GetExp(CachedVoiceChannel channel, TimeSpan period)
        {
            var xp = Convert.ToInt32(period.TotalMinutes * 2);
            if (IsReducedExp(channel)) xp = Convert.ToInt32(xp / 10);
            return xp;
        }

        private bool IsReducedExp(CachedTextChannel channel)
        {
            var isChannel = ServerTextChanReduction.TryGetValue(channel.Guild.Id.RawValue, out var channels);
            var isCategory = ServerCategoryReduction.TryGetValue(channel.Guild.Id.RawValue, out var category);
            return !isCategory
                ? isChannel && channels.TryGetValue(channel.Id.RawValue, out _)
                : !channel.CategoryId.HasValue
                    ? isChannel && channels.TryGetValue(channel.Id.RawValue, out _)
                    : category.TryGetValue(channel.CategoryId.Value, out _) ||
                      isChannel && channels.TryGetValue(channel.Id.RawValue, out _);
        }

        private bool IsReducedExp(CachedVoiceChannel channel)
        {
            var isChannel = ServerVoiceChanReduction.TryGetValue(channel.Guild.Id.RawValue, out var channels);
            var isCategory = ServerCategoryReduction.TryGetValue(channel.Guild.Id.RawValue, out var category);
            return !isCategory
                ? isChannel && channels.TryGetValue(channel.Id.RawValue, out _)
                : !channel.CategoryId.HasValue
                    ? isChannel && channels.TryGetValue(channel.Id.RawValue, out _)
                    : category.TryGetValue(channel.CategoryId.Value, out _) ||
                      isChannel && channels.TryGetValue(channel.Id.RawValue, out _);
        }

        private async Task MvpCount(DbService db, Account userData, CachedMember user)
        {
            var cfg = await db.GetOrCreateGuildConfigAsync(user.Guild);
            if (!cfg.Premium) return;
            userData.MvpCount++;
            await db.SaveChangesAsync();
        }

        private async Task GiveawayAsync(DbService db, CachedMember user)
        {
            var giveaways = await db.Giveaways
                .Where(x => x.GuildId == user.Guild.Id.RawValue && x.Type == GiveawayType.Activity && x.Active)
                .ToListAsync();
            if (giveaways.Count == 0) return;
            for (var i = 0; i < giveaways.Count; i++)
            {
                var x = giveaways[i];
                if (!x.Active) continue;
                if (x.CloseAtOffset.HasValue && x.CloseAtOffset.Value >= DateTimeOffset.UtcNow) continue;
                if (!x.Stack)
                {
                    var check = await db.GiveawayParticipants.FirstOrDefaultAsync(e =>
                        e.UserId == user.Id.RawValue && e.GiveawayId == x.Id);
                    if(check != null) continue;
                }

                await db.GiveawayParticipants.AddAsync(new GiveawayParticipant
                {
                    Id = Guid.NewGuid(),
                    GuildId = user.Guild.Id.RawValue,
                    UserId = user.Id.RawValue,
                    Entry = DateTimeOffset.UtcNow,
                    GiveawayId = x.Id,
                    Giveaway = x
                });
            }

            await db.SaveChangesAsync();
        }
    }
}