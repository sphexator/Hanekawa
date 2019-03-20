﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Config;
using Hanekawa.Addons.Perspective;
using Hanekawa.Entities;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Events;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Administration;
using Hanekawa.Services.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Services.AutoModerator
{
    public class NudeScoreService : IHanaService, IRequiredService
    {
        private readonly Timer _cleanupTimer;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly LogService _log;
        private readonly ModerationService _moderationService;
        private readonly Timer _moveToLongTerm;
        private readonly MuteService _muteService;
        private readonly PerspectiveClient _perspectiveClient;
        private readonly string _perspectiveToken;
        private readonly Timer _quickClearSingle;
        private readonly WarnService _warnService;

        public NudeScoreService(DiscordSocketClient client, IConfiguration config, ModerationService moderationService,
            WarnService warnService, MuteService muteService, LogService log)
        {
            _client = client;
            _config = config;
            _moderationService = moderationService;
            _warnService = warnService;
            _muteService = muteService;
            _log = log;
            _perspectiveClient = new PerspectiveClient();

            _perspectiveToken = _config["perspective"];

            _client.MessageReceived += DetermineNudeScore;

            using (var db = new DbService())
            {
                foreach (var x in db.NudeServiceChannels)
                {
                    var guild = NudeChannels.GetOrAdd(x.GuildId, new ConcurrentDictionary<ulong, int>());
                    guild.GetOrAdd(x.ChannelId, x.Tolerance);
                }

                foreach (var x in db.SingleNudeServiceChannels)
                {
                    var guild = SingleNudeChannels.GetOrAdd(x.GuildId,
                        new ConcurrentDictionary<ulong, SingleNudeServiceChannel>());
                    guild.GetOrAdd(x.ChannelId, x);
                }
            }

            _cleanupTimer = new Timer(__ =>
            {
                foreach (var a in FastNudeValue)
                foreach (var y in a.Value)
                foreach (var z in y.Value)
                foreach (var x in z.Value)
                {
                    if (x.Time.AddHours(1) > DateTime.UtcNow) continue;
                    z.Value.Remove(x);
                }
            }, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            _moveToLongTerm = new Timer(_ =>
            {
                foreach (var a in SlowNudeValue)
                foreach (var y in a.Value)
                foreach (var z in y.Value)
                foreach (var x in z.Value)
                {
                    if (x.Time.AddHours(24) > DateTime.UtcNow) continue;
                    z.Value.Remove(x);
                }
            }, null, TimeSpan.FromDays(1), TimeSpan.FromHours(1));

            _quickClearSingle = new Timer(_ =>
            {
                foreach (var a in SingleNudeValue)
                foreach (var y in a.Value)
                foreach (var x in y.Value)
                {
                    if (x.Value.Time.AddMinutes(10) > DateTime.UtcNow) continue;
                    y.Value.Remove(x.Key, out var _);
                }
            }, null, TimeSpan.FromHours(1), TimeSpan.FromMinutes(1));
            Console.WriteLine("Nudescore service loaded");
        }

        // Short-term caching of values for Auto-moderator to view
        private ConcurrentDictionary<ulong,
                ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>>>
            FastNudeValue { get; }
            = new ConcurrentDictionary<ulong,
                ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>>>();

        // Long-term caching of values for moderators to view
        private ConcurrentDictionary<ulong,
                ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>>>
            SlowNudeValue { get; }
            = new ConcurrentDictionary<ulong,
                ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>>>();

        // Single caching of values for auto-moderator to view
        private ConcurrentDictionary<ulong,
                ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ToxicityEntry>>>
            SingleNudeValue { get; }
            = new ConcurrentDictionary<ulong,
                ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ToxicityEntry>>>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, int>> NudeChannels { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, int>>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SingleNudeServiceChannel>>
            SingleNudeChannels { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SingleNudeServiceChannel>>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, int>> WarnAmount { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, int>>();

        private ConcurrentDictionary<ulong, LinkedList<Timer>> WarnTimer { get; }
            = new ConcurrentDictionary<ulong, LinkedList<Timer>>();

        public event AsyncEvent<SocketGuildUser, ModerationService.AutoModActionType, string, double, double>
            AutoModFilter;

        public double GetSingleScore(ITextChannel channel, IUser user)
        {
            if (!SlowNudeValue.TryGetValue(channel.GuildId, out var channels)) return 0;
            if (!channels.TryGetValue(channel.Id, out var users)) return 0;
            if (!users.TryGetValue(user.Id, out var list)) return 0;

            double score = 0;
            foreach (var x in list) score += x.Value;

            return score / list.Count;
        }

        public string GetAllScores(SocketGuildUser user)
        {
            string result = null;
            if (!SlowNudeValue.TryGetValue(user.Guild.Id, out var channels)) return null;
            foreach (var y in channels)
            {
                if (!y.Value.TryGetValue(user.Id, out var list)) continue;
                double score = 0;
                foreach (var x in list) score += x.Value;
                result += $"Toxicity score in {user.Guild.GetTextChannel(y.Key).Mention}: {score / list.Count}\n";
            }

            return result;
        }

        public string GetChannelTopScores(ITextChannel channel)
        {
            var result = new StringBuilder();
            var chanUsers = new Dictionary<ulong, double>();
            if (!SlowNudeValue.TryGetValue(channel.GuildId, out var channels)) return null;
            if (!channels.TryGetValue(channel.Id, out var users)) return null;
            foreach (var y in users)
            {
                double score = 0;
                foreach (var x in y.Value) score += x.Value;

                chanUsers.TryAdd(y.Key, score / y.Value.Count);
            }

            var topUsers = chanUsers.OrderBy(x => x.Value).Take(5);
            foreach (var (id, score) in topUsers) result.AppendLine($"{_client.GetUser(id)} Score: {score}");
            return result.ToString();
        }

        public IEnumerable<string> GetGuildTopScore(IGuild guild)
        {
            var result = new List<string>();
            if (!SlowNudeValue.TryGetValue(guild.Id, out var channels)) return null;
            foreach (var a in channels)
            {
                var channel = _client.GetGuild(guild.Id).GetTextChannel(a.Key);
                if (channel == null) continue;
                var userScore = new Dictionary<ulong, double>();
                foreach (var y in a.Value)
                {
                    double score = 0;
                    foreach (var x in y.Value) score += x.Value;

                    userScore.TryAdd(y.Key, score / y.Value.Count);
                }

                var page = new StringBuilder();
                page.AppendLine($"{channel.Mention}");
                var topUsers = userScore.OrderBy(x => x.Value).Take(5);
                foreach (var (id, score) in topUsers) page.AppendLine($"{_client.GetUser(id).Mention ?? $"User left server ({id})"} Score: {score}");
                result.Add(page.ToString());
            }

            return result;
        }

        public async Task<EmbedBuilder> SetNudeChannel(ITextChannel ch, int tolerance)
        {
            var guild = NudeChannels.GetOrAdd(ch.GuildId, new ConcurrentDictionary<ulong, int>());
            guild.AddOrUpdate(ch.Id, tolerance, (key, old) => tolerance);
            using (var db = new DbService())
            {
                var check = await db.NudeServiceChannels.FindAsync(ch.GuildId, ch.Id);
                if (check != null)
                {
                    check.Tolerance = tolerance;
                    await db.SaveChangesAsync();
                    return new EmbedBuilder().CreateDefault(
                        $"Updated {ch.Mention} average toxicity filter tolerance to {tolerance}", Color.Green.RawValue);
                }

                var data = new NudeServiceChannel
                {
                    GuildId = ch.GuildId,
                    ChannelId = ch.Id,
                    Tolerance = tolerance
                };
                await db.NudeServiceChannels.AddAsync(data);
                await db.SaveChangesAsync();
                return new EmbedBuilder().CreateDefault(
                    $"Added {ch.Mention} to average toxicity filter with tolerance of {tolerance}",
                    Color.Green.RawValue);
            }
        }

        public async Task<EmbedBuilder> SetSingleNudeChannel(ITextChannel ch, int level, int tolerance)
        {
            var guild = SingleNudeChannels.GetOrAdd(ch.GuildId,
                new ConcurrentDictionary<ulong, SingleNudeServiceChannel>());
            var cfg = new SingleNudeServiceChannel
            {
                GuildId = ch.GuildId,
                ChannelId = ch.Id,
                Level = level,
                Tolerance = tolerance
            };
            guild.AddOrUpdate(ch.Id, cfg, (key, old) => cfg);
            using (var db = new DbService())
            {
                var check = await db.SingleNudeServiceChannels.FindAsync(ch.GuildId, ch.Id);
                if (check != null)
                {
                    check.Tolerance = tolerance;
                    check.Level = level;
                    await db.SaveChangesAsync();
                    return new EmbedBuilder().CreateDefault(
                        $"Updated {ch.Mention} single toxicity filter to {tolerance} for people lvl{level} or below.",
                        Color.Green.RawValue);
                }

                await db.SingleNudeServiceChannels.AddAsync(cfg);
                await db.SaveChangesAsync();
                return new EmbedBuilder().CreateDefault(
                    $"Added {ch.Mention} to single toxicity filter with tolerance of {tolerance} for people lvl{level} or below.",
                    Color.Green.RawValue);
            }
        }

        public async Task<EmbedBuilder> RemoveNudeChannel(ITextChannel ch)
        {
            if (!NudeChannels.TryGetValue(ch.GuildId, out var channels)) return null;
            if (!channels.TryRemove(ch.Id, out var value)) return null;
            using (var db = new DbService())
            {
                var check = await db.NudeServiceChannels.FindAsync(ch.GuildId, ch.Id);
                if (check == null) return null;
                db.NudeServiceChannels.Remove(check);
                return new EmbedBuilder().CreateDefault($"Removed average toxicity filter from {ch.Mention}",
                    Color.Green.RawValue);
            }
        }

        public async Task<EmbedBuilder> RemoveSingleNudeChannel(ITextChannel ch)
        {
            if (!SingleNudeChannels.TryGetValue(ch.GuildId, out var channels))
                return null;
            if (!channels.TryRemove(ch.Id, out _))
                return null;
            using (var db = new DbService())
            {
                var check = await db.SingleNudeServiceChannels.FindAsync(ch.GuildId, ch.Id);
                if (check == null)
                    return null;
                db.SingleNudeServiceChannels.Remove(check);
                return new EmbedBuilder().CreateDefault($"Removed single toxicity filter from {ch.Mention}",
                    Color.Green.RawValue);
            }
        }

        private Task DetermineNudeScore(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (msg.Author.IsBot) return;

                if (!(msg is SocketUserMessage message)) return;

                if (message.Source != MessageSource.User) return;

                if (!(message.Author is SocketGuildUser user)) return;

                if (!(message.Channel is SocketTextChannel ch)) return;

                var response = await _perspectiveClient.GetToxicityScore(FilterMessage(msg.Content), _perspectiveToken);
                var score = response.AttributeScores.TOXICITY.SummaryScore.Value * 100;
                var single = SingleMessageAsync(score, user, ch, message);
                var multi = MultiMessageProcessingAsync(score, user, ch, message);
                await Task.WhenAll(single, multi);
            });
            return Task.CompletedTask;
        }

        private async Task SingleMessageAsync(double score, IGuildUser user, SocketTextChannel ch,
            SocketUserMessage msg)
        {
            if (!SingleNudeChannels.TryGetValue(user.GuildId, out var channels)) return;
            if (!channels.TryGetValue(ch.Id, out var cfg)) return;
            _log.LogAction(LogLevel.Information, $"{user.Id} in {user.Guild.Id} scored {score} in toxicity",
                "Toxicity");
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);

                var toxList = SingleNudeValue.GetOrAdd(user.GuildId,
                    new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ToxicityEntry>>());
                var channelValue = toxList.GetOrAdd(ch.Id, new ConcurrentDictionary<ulong, ToxicityEntry>());
                var userValue = channelValue.GetOrAdd(user.Id, new ToxicityEntry());
                channelValue.AddOrUpdate(user.Id,
                    new ToxicityEntry {Time = DateTime.UtcNow, Value = score, MessageId = msg.Id},
                    (key, value) => new ToxicityEntry {Time = DateTime.UtcNow, Value = score, MessageId = msg.Id});
                if (userValue == null) return;

                var totalScore = (score + userValue.Value) / 2;
                int tolerance;
                int? level = null;

                if (cfg.Level.HasValue && cfg.Tolerance.HasValue)
                {
                    level = cfg.Level.Value;
                    tolerance = cfg.Tolerance.Value;
                }
                else if (cfg.InHouse)
                {
                    tolerance = InHouseSingleToxicityTolerance(userdata.Level);
                }
                else
                {
                    return;
                }

                if (totalScore > tolerance && level.HasValue && level.Value <= cfg.Level.Value)
                {
                }
                else if (totalScore > tolerance)
                {
                }
                else
                {
                    return;
                }

                try
                {
                    var del = new List<IMessage>();
                    var oldMsg = await ch.GetMessageAsync(userValue.MessageId);
                    if (oldMsg != null) del.Add(oldMsg);
                    del.Add(msg);
                    await ch.DeleteMessagesAsync(del);
                    _log.LogAction(LogLevel.Information,
                        $"Single Toxicity: {user.Id} scored a toxicity value of {totalScore} (cap {tolerance} lvl:{level}) over two messages in {user.Guild.Id}",
                        "SingleToxicity");
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, $"{e.Message}\n{e.StackTrace}", "SingleToxicity");
                }

                await AutoModFilter(user as SocketGuildUser, ModerationService.AutoModActionType.Toxicity, msg.Content,
                    totalScore, tolerance);
            }
        }

        private async Task MultiMessageProcessingAsync(double score, IGuildUser user, SocketTextChannel ch,
            SocketUserMessage msg)
        {
            if (!NudeChannels.TryGetValue(ch.Guild.Id, out var channels)) return;
            if (!channels.TryGetValue(ch.Id, out var channel)) return;
            SlowNudeValue.ToxicityAdd(score, user, ch, msg);
            var result = FastNudeValue.ToxicityAdd(score, user, ch, msg);
            if (result == null) return;
            _log.LogAction(LogLevel.Information, $"{user.Id} in {user.Guild.Id} scored {score} in toxicity",
                "Toxicity");
            if (result < channel) return;
            await NudeWarn(user as SocketGuildUser, ch).ConfigureAwait(false);
        }

        private void ClearChannelNudeScore(IGuildUser user, SocketTextChannel channel)
        {
            var toxList = FastNudeValue.GetOrAdd(user.GuildId,
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>>());
            var userValue = toxList.GetOrAdd(user.Id, new ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>());
            var channelValue = userValue.GetOrAdd(channel.Id, new LinkedList<ToxicityEntry>());
            channelValue.Clear();
        }

        private void StartWarnTimer(IGuildUser user)
        {
            var toAdd = new Timer(_ =>
            {
                var guildWarn = WarnAmount.GetOrAdd(user.GuildId, new ConcurrentDictionary<ulong, int>());
                guildWarn.AddOrUpdate(user.Id, 0, (key, old) => old = old - 1);

                var timers = WarnTimer.GetOrAdd(user.Id, new LinkedList<Timer>());
                timers.Last.Value.Change(Timeout.Infinite, Timeout.Infinite);
                timers.RemoveLast();
            }, null, new TimeSpan(1, 0, 0), Timeout.InfiniteTimeSpan);

            var timer = WarnTimer.GetOrAdd(user.Id, new LinkedList<Timer>());
            timer.AddFirst(toAdd);
        }

        private async Task NudeWarn(SocketGuildUser user, SocketTextChannel channel)
        {
            var guildWarn = WarnAmount.GetOrAdd(user.Guild.Id, new ConcurrentDictionary<ulong, int>());
            var userWarn = guildWarn.GetOrAdd(user.Id, 0);

            switch (userWarn + 1)
            {
                case 1:
                    guildWarn.AddOrUpdate(user.Id, 1, (key, old) => old = old + 1);
                    StartWarnTimer(user);
                    ClearChannelNudeScore(user, channel);
                    await _warnService.AddWarning(user, 1, DateTime.UtcNow, $"High toxicity score in {channel.Name}",
                        WarnReason.Warning, (await channel.GetMessagesAsync().FlattenAsync())
                        .Where(m => m.Author.Id == user.Id)
                        .Take(100).ToArray().ToList());
                    _log.LogAction(LogLevel.Information,
                        $"Multi Toxicity: {user.Id} scored over avg. threshold in toxicity in {user.Guild.Id} - Silent Warned",
                        "MultiToxicity");
                    break;

                case 2:
                    guildWarn.AddOrUpdate(user.Id, 1, (key, old) => old = old + 1);
                    StartWarnTimer(user);
                    ClearChannelNudeScore(user, channel);
                    await channel.SendMessageAsync($"{user.Mention} please calm down (warning)").ConfigureAwait(false);
                    _log.LogAction(LogLevel.Information,
                        $"Multi Toxicity: {user.Id} scored over avg. threshold in toxicity in {user.Guild.Id} - Visibly Warned",
                        "MultiToxicity");
                    break;

                case 3:
                    guildWarn.AddOrUpdate(user.Id, 1, (key, old) => old = old + 1);
                    StartWarnTimer(user);
                    ClearChannelNudeScore(user, channel);
                    await _warnService.AddWarning(user, 1, DateTime.UtcNow, $"High toxicity score in {channel.Name}",
                        WarnReason.Mute, TimeSpan.FromHours(1), (await channel.GetMessagesAsync().FlattenAsync())
                        .Where(m => m.Author.Id == user.Id)
                        .Take(100).ToArray().ToList(), true);
                    await _moderationService.AutoModMute(user, ModerationService.AutoModActionType.Toxicity,
                        TimeSpan.FromHours(1), $"High toxicity score in {channel.Name}");
                    _log.LogAction(LogLevel.Information,
                        $"Multi Toxicity: {user.Id} scored over avg. threshold in toxicity in {user.Guild.Id} - Muted",
                        "MultiToxicity");
                    break;
            }
        }

        private static string FilterMessage(string content)
        {
            var emote = new Regex(@"/^<a?:(\w+):(\d+)>$/",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var channel = new Regex(@"/^<#(\d+)>$/",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var mention = new Regex(@"/^<@!?(\d+)>$/",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var role = new Regex(@"/^<@&(\d+)>$/",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var emoteFilter = emote.Replace(content, "");
            var channelFilter = channel.Replace(emoteFilter, "");
            var mentionFilter = mention.Replace(channelFilter, "");
            var roleFilter = role.Replace(mentionFilter, "");
            return roleFilter;
        }

        private static int InHouseAverageToxicityTolerance(int level) => 60 + level * level / 80;

        private static int InHouseSingleToxicityTolerance(int level) => 70 + level * level / 160;
    }
}