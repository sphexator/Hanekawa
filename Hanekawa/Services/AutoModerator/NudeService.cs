using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Addons.Perspective;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Hanekawa.Services.Administration;
using Microsoft.Extensions.Configuration;

namespace Hanekawa.Services.AutoModerator
{
    public class NudeScoreService
    {
        private readonly Timer _cleanupTimer;
        private readonly Timer _MoveToLongTerm;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly ModerationService _moderationService;
        private readonly MuteService _muteService;
        private readonly PerspectiveClient _perspectiveClient;
        private readonly string _perspectiveToken;
        private readonly WarnService _warnService;

        public NudeScoreService(DiscordSocketClient client, IConfiguration config, ModerationService moderationService,
            WarnService warnService, MuteService muteService)
        {
            _client = client;
            _config = config;
            _moderationService = moderationService;
            _warnService = warnService;
            _muteService = muteService;

            _perspectiveToken = _config["perspective"];

            _client.MessageReceived += DetermineNudeScore;

            using (var db = new DbService())
            {
                foreach (var x in db.NudeServiceChannels)
                {
                    var guild = NudeChannels.GetOrAdd(x.GuildId, new ConcurrentDictionary<ulong, uint>());
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
                    
                    var toxList = SlowNudeValue.GetOrAdd(a.Key,
                        new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>>());
                    var channel = toxList.GetOrAdd(y.Key, new ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>());
                    var userValue = channel.GetOrAdd(z.Key, new LinkedList<ToxicityEntry>());
                    
                    if (userValue.Count == 20)
                    {
                        userValue.RemoveLast();
                        userValue.AddFirst(new ToxicityEntry {Value = x.Value, Time = x.Time});
                    }
                    else
                    {
                        userValue.AddFirst(new ToxicityEntry {Value = x.Value, Time = x.Time});
                    }

                    z.Value.Remove(x);
                }
            }, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            _MoveToLongTerm = new Timer(_ =>
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

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>> NudeChannels { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SingleNudeServiceChannel>>
            SingleNudeChannels { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SingleNudeServiceChannel>>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>> WarnAmount { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>>();

        private ConcurrentDictionary<ulong, LinkedList<Timer>> WarnTimer { get; }
            = new ConcurrentDictionary<ulong, LinkedList<Timer>>();

        public async Task<EmbedBuilder> SetNudeChannel(ITextChannel ch, uint tolerance)
        {
            var guild = NudeChannels.GetOrAdd(ch.GuildId, new ConcurrentDictionary<ulong, uint>());
            guild.AddOrUpdate(ch.Id, tolerance, (key, old) => tolerance);
            using (var db = new DbService())
            {
                var check = await db.NudeServiceChannels.FindAsync(ch.GuildId, ch.Id);
                if (check != null)
                {
                    check.Tolerance = tolerance;
                    await db.SaveChangesAsync();
                    return new EmbedBuilder().Reply(
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
                return new EmbedBuilder().Reply(
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
                    return new EmbedBuilder().Reply(
                        $"Updated {ch.Mention} single toxicity filter to {tolerance} for people lvl{level} or below.",
                        Color.Green.RawValue);
                }

                await db.SingleNudeServiceChannels.AddAsync(cfg);
                await db.SaveChangesAsync();
                return new EmbedBuilder().Reply(
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
                return new EmbedBuilder().Reply($"Removed average toxicity filter from {ch.Mention}",
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
                return new EmbedBuilder().Reply($"Removed single toxicity filter from {ch.Mention}",
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
                var multi = MultiMessageProcessingAsync(score, user, ch);
                await Task.WhenAll(single, multi);
            });
            return Task.CompletedTask;
        }

        private double? CalculateNudeScore(double result, IGuildUser user, SocketTextChannel channel)
        {
            var toxList = FastNudeValue.GetOrAdd(user.GuildId,
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>>());
            var channelValue = toxList.GetOrAdd(user.Id, new ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>());
            var userValue = channelValue.GetOrAdd(channel.Id, new LinkedList<ToxicityEntry>());

            if (channelValue.Count == 20)
            {
                userValue.RemoveLast();
                userValue.AddFirst(new ToxicityEntry {Value = result, Time = DateTime.UtcNow});
            }
            else
            {
                userValue.AddFirst(new ToxicityEntry {Value = result, Time = DateTime.UtcNow});
                return null;
            }

            double totalScore = 0;

            foreach (var x in userValue) totalScore = x.Value + totalScore;

            return totalScore / channelValue.Count;
        }

        private async Task SingleMessageAsync(double score, IGuildUser user, SocketTextChannel ch,
            IDeletable msg)
        {
            var isChannels = SingleNudeChannels.TryGetValue(user.GuildId, out var channels);
            if (!isChannels) return;

            var isEnabled = channels.TryGetValue(ch.Id, out var cfg);
            if (!isEnabled) return;

            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);
                if (userdata.Level > cfg.Level) return;

                if (score <= cfg.Tolerance) return;

                try
                {
                    await msg.DeleteAsync();
                }
                catch
                {
                    // ignored
                }
            }
        }

        private async Task MultiMessageProcessingAsync(double score, IGuildUser user, SocketTextChannel ch)
        {
            if (!NudeChannels.TryGetValue(ch.Guild.Id, out var channels)) return;

            if (!channels.TryGetValue(ch.Id, out var channel)) return;

            var result = CalculateNudeScore(score, user, ch);
            if (result == null) return;

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
                var guildWarn = WarnAmount.GetOrAdd(user.GuildId, new ConcurrentDictionary<ulong, uint>());
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
            var guildWarn = WarnAmount.GetOrAdd(user.Guild.Id, new ConcurrentDictionary<ulong, uint>());
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
                    break;

                case 2:
                    guildWarn.AddOrUpdate(user.Id, 1, (key, old) => old = old + 1);
                    StartWarnTimer(user);
                    ClearChannelNudeScore(user, channel);
                    await channel.SendMessageAsync($"{user.Mention} please calm down (warning)").ConfigureAwait(false);
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
    }
}