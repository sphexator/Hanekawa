using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Services.Administration;
using Hanekawa.Services.AutoModerator.Perspective.Models;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quartz.Util;

namespace Hanekawa.Services.AutoModerator
{
    public class NudeScoreService
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly MuteService _muteService;
        private readonly string _perspectiveToken;
        private readonly WarnService _warnService;
        private readonly ModerationService _moderationService;

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<double>>>> NudeValue { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<double>>>>();
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>> NudeChannels { get; set; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>> WarnAmount { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>>();
        private ConcurrentDictionary<ulong, LinkedList<Timer>> WarnTimer { get; }
            = new ConcurrentDictionary<ulong, LinkedList<Timer>>();

        public NudeScoreService(DiscordSocketClient client, IConfiguration config, ModerationService moderationService, WarnService warnService, MuteService muteService)
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
            }
        }

        public async Task SetNudeChannel(ITextChannel ch, uint tolerance)
        {
            var guild = NudeChannels.GetOrAdd(ch.GuildId, new ConcurrentDictionary<ulong, uint>());
            guild.AddOrUpdate(ch.Id, tolerance, (key, old) => old = tolerance);
            using (var db = new DbService())
            {
                var check = await db.NudeServiceChannels.FindAsync(ch.GuildId, ch.Id);
                if (check != null)
                {
                    check.Tolerance = tolerance;
                    await db.SaveChangesAsync();
                }
                else
                {
                    var data = new NudeServiceChannel
                    {
                        GuildId = ch.GuildId,
                        ChannelId = ch.Id,
                        Tolerance = tolerance
                    };
                    await db.NudeServiceChannels.AddAsync(data);
                    await db.SaveChangesAsync();
                }
            }
        }

        private Task DetermineNudeScore(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (msg.Author.IsBot) return;
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                if (!(message.Author is SocketGuildUser)) return;
                if (!(message.Channel is SocketTextChannel)) return;
                if (!NudeChannels.TryGetValue((msg.Channel as SocketTextChannel).Guild.Id, out var channels)) return;
                if (!channels.TryGetValue(msg.Channel.Id, out var channel)) return;
                var content = msg.Content;
                var filter = FilterMessage(content);
                if (filter.IsNullOrWhiteSpace()) return;
                var request = new AnalyzeCommentRequest(filter);

                var response = await SendNudes(request);
                var score = response.AttributeScores.TOXICITY.SummaryScore.Value;
                var result = CalculateNudeScore(score, msg.Author as SocketGuildUser, msg.Channel as SocketTextChannel);
                if (result == null) return;
                if (result < channel) return;
                await NudeWarn(msg.Author as SocketGuildUser, msg.Channel as SocketTextChannel).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }

        private async Task<AnalyzeCommentResponse> SendNudes(AnalyzeCommentRequest request)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8,
                    "application/json");
                var response = await client
                    .PostAsync(
                        $"https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key={_perspectiveToken}",
                        content);
                response.EnsureSuccessStatusCode();
                var data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<AnalyzeCommentResponse>(data);
                return result;
            }
        }

        private double? CalculateNudeScore(double doubleScore, IGuildUser user, SocketTextChannel channel)
        {
            var toxList = NudeValue.GetOrAdd(user.GuildId,
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<double>>>());
            var channelValue = toxList.GetOrAdd(user.Id, new ConcurrentDictionary<ulong, LinkedList<double>>());
            var userValue = channelValue.GetOrAdd(channel.Id, new LinkedList<double>());

            var result = doubleScore * 100;

            if (channelValue.Count == 20)
            {
                userValue.RemoveLast();
                userValue.AddFirst(result);
            }
            else
            {
                userValue.AddFirst(result);
                return null;
            }

            double totalScore = 0;

            foreach (var x in userValue) totalScore = x + totalScore;

            return totalScore / channelValue.Count;
        }

        private void ClearChannelNudeScore(IGuildUser user, SocketTextChannel channel)
        {
            var toxList = NudeValue.GetOrAdd(user.GuildId,
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<double>>>());
            var userValue = toxList.GetOrAdd(user.Id, new ConcurrentDictionary<ulong, LinkedList<double>>());
            var channelValue = userValue.GetOrAdd(channel.Id, new LinkedList<double>());
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
                    await _moderationService.AutoModMute(user as SocketGuildUser, ModerationService.AutoModActionType.Toxicity,
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