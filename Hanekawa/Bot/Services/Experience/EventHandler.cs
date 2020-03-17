using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService : INService, IRequired
    {
        private readonly DiscordBot _client;
        private readonly InternalLogService _log;
        private readonly Random _random;

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerCategoryReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerTextChanReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerVoiceChanReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public ExpService(DiscordBot client, Random random, InternalLogService log)
        {
            _client = client;
            _random = random;
            _log = log;

            _ = EventHandler(new CancellationToken());

            _client.MessageReceived += ServerMessageExpAsync;
            _client.MessageReceived += GlobalMessageExpAsync;
            _client.VoiceStateUpdated += VoiceExpAsync;
            _client.MemberJoined += GiveRolesBackAsync;

            using (var db = new DbService())
            {
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
                        _log.LogAction(LogLevel.Error, e, $"Couldn't load {x.GuildId} reward plugin for {x.ChannelId}, remove?");
                    }
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
                    using var db = new DbService();
                    var userdata = await db.GetOrCreateGlobalUserData(user);
                    await AddExpAsync(userdata, GetExp(channel), _random.Next(1, 3), db);
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Exp Service) Error in {user.Guild.Id} for Global Exp - {e.Message}");
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
                    using var db = new DbService();
                    var userData = await db.GetOrCreateUserData(user);
                    userData.LastMessage = DateTime.UtcNow;
                    if (!userData.FirstMessage.HasValue) userData.FirstMessage = DateTime.UtcNow;

                    await AddExpAsync(user, userData, GetExp(channel), _random.Next(1, 3), db);
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Exp Service) Error in {user.Guild.Id} for Server Exp - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task VoiceExpAsync(VoiceStateUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.Member;
                var after = e.NewVoiceState;
                var before = e.OldVoiceState;
                try
                {
                    using var db = new DbService();
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
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Exp Service) Error in {user.Guild.Id} for Voice - {e.Message}");
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
            var isChannel = ServerTextChanReduction.TryGetValue(channel.Guild.Id, out var channels);
            var isCategory = ServerCategoryReduction.TryGetValue(channel.Guild.Id, out var category);
            if (!isCategory) return isChannel && channels.TryGetValue(channel.Id, out _);
            if (!channel.CategoryId.HasValue) return isChannel && channels.TryGetValue(channel.Id, out _);
            if (category.TryGetValue(channel.CategoryId.Value, out _))
                return true;
            return isChannel && channels.TryGetValue(channel.Id, out _);
        }

        private bool IsReducedExp(CachedVoiceChannel channel)
        {
            var isChannel = ServerVoiceChanReduction.TryGetValue(channel.Guild.Id, out var channels);
            var isCategory = ServerCategoryReduction.TryGetValue(channel.Guild.Id, out var category);
            if (!isCategory) return isChannel && channels.TryGetValue(channel.Id, out _);
            if (!channel.CategoryId.HasValue) return isChannel && channels.TryGetValue(channel.Id, out _);
            if (category.TryGetValue(channel.CategoryId.Value, out _))
                return true;
            return isChannel && channels.TryGetValue(channel.Id, out _);
        }
    }
}