using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly InternalLogService _log;
        private readonly Random _random;

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerCategoryReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerTextChanReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerVoiceChanReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public ExpService(DiscordSocketClient client, Random random, InternalLogService log)
        {
            _client = client;
            _random = random;
            _log = log;

            using (var db = new DbService())
            {
                foreach (var x in db.LevelExpReductions)
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
                            throw new ArgumentOutOfRangeException();
                    }
                }

                foreach (var x in db.LevelConfigs)
                {
                    _textExpMultiplier.TryAdd(x.GuildId, x.TextExpMultiplier);
                    _voiceExpMultiplier.TryAdd(x.GuildId, x.VoiceExpMultiplier);
                }
            }

            _ = EventHandler(new CancellationToken());

            _client.MessageReceived += ServerMessageExpAsync;
            _client.MessageReceived += GlobalMessageExpAsync;
            _client.UserVoiceStateUpdated += VoiceExpAsync;
            _client.UserJoined += GiveRolesBackAsync;
        }

        private Task GlobalMessageExpAsync(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                if (!(msg.Author is SocketGuildUser user)) return;
                if (!(msg.Channel is SocketTextChannel channel)) return;
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

        private Task ServerMessageExpAsync(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                if (!(msg.Author is SocketGuildUser user)) return;
                if (!(msg.Channel is SocketTextChannel channel)) return;
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

        private Task VoiceExpAsync(SocketUser usr, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(async () =>
            {
                if (!(usr is SocketGuildUser user)) return;
                try
                {
                    using var db = new DbService();
                    var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);
                    if (!cfg.VoiceExpEnabled) return;
                    if (before.VoiceChannel == null && after.VoiceChannel != null)
                    {
                        var userData = await db.GetOrCreateUserData(user);
                        userData.VoiceExpTime = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        return;
                    }

                    if (before.VoiceChannel != null && after.VoiceChannel == null)
                    {
                        var userData = await db.GetOrCreateUserData(user);
                        var exp = GetExp(before.VoiceChannel, DateTime.UtcNow - userData.VoiceExpTime);
                        await AddExpAsync(user, userData, exp, exp / 2, db);
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

        private int GetExp(SocketTextChannel channel)
        {
            var xp = _random.Next(10, 20);
            if (IsReducedExp(channel)) xp = Convert.ToInt32(xp / 10);
            return xp;
        }

        private int GetExp(SocketVoiceChannel channel, TimeSpan period)
        {
            var xp = Convert.ToInt32(period.TotalMinutes * 2);
            if (IsReducedExp(channel)) xp = Convert.ToInt32(xp / 10);
            return xp;
        }

        private bool IsReducedExp(SocketTextChannel channel)
        {
            var isChannel = ServerTextChanReduction.TryGetValue(channel.Guild.Id, out var channels);
            var isCategory = ServerCategoryReduction.TryGetValue(channel.Guild.Id, out var category);
            if (!isCategory) return isChannel && channels.TryGetValue(channel.Id, out _);
            if (!channel.CategoryId.HasValue) return isChannel && channels.TryGetValue(channel.Id, out _);
            if (category.TryGetValue(channel.CategoryId.Value, out _))
                return true;
            return isChannel && channels.TryGetValue(channel.Id, out _);
        }

        private bool IsReducedExp(SocketVoiceChannel channel)
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