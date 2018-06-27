using Discord;
using Discord.WebSocket;
using Jibril.Services.Administration;
using Jibril.Services.AutoModerator.Perspective.Models;
using Jibril.Services.Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quartz.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jibril.Services.AutoModerator
{
    public class NudeScoreService
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly MuteService _muteService;
        private readonly string _perspectiveToken;
        private readonly WarnService _warnService;

        public NudeScoreService(DiscordSocketClient client, HttpClient httpClient, IConfiguration config)
        {
            _client = client;
            _httpClient = httpClient;
            _config = config;

            _perspectiveToken = _config["perspective"];

            _client.MessageReceived += DetermineNudeScore;
        }

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<uint>>>>
            NudeValue { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<uint>>>
            >();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>> WarnAmount { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, uint>>();

        private ConcurrentDictionary<ulong, LinkedList<Timer>> WarnTimer { get; }
            = new ConcurrentDictionary<ulong, LinkedList<Timer>>();

        private Task DetermineNudeScore(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                if (!(message.Author is SocketGuildUser)) return;
                if (!(message.Channel is SocketTextChannel)) return;
                var content = msg.Content;
                var filter = FilterMessage(content);
                if (filter.IsNullOrWhiteSpace()) return;
                var request = new AnalyzeCommentRequest(filter);

                var response = SendNudes(request);
                var score = response.AttributeScores.TOXICITY.SummaryScore.Value;
                var result = CalculateNudeScore(score, msg.Author as SocketGuildUser, msg.Channel as SocketTextChannel);
                if (result == null) return;
                using (var db = new DbService())
                {
                    var channel = await db.NudeServiceChannels.FindAsync((msg.Author as SocketGuildUser).Guild.Id, msg.Channel.Id).ConfigureAwait(false);
                    if (channel == null) return;
                    if (result < channel.Tolerance) return;
                }
                await NudeWarn(msg.Author as SocketGuildUser, msg.Channel as SocketTextChannel).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }

        private AnalyzeCommentResponse SendNudes(AnalyzeCommentRequest request)
        {
            using (var client = _httpClient)
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8,
                    "application/json");
                var response = client
                    .PostAsync(
                        $"https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key={_perspectiveToken}",
                        content).Result;
                response.EnsureSuccessStatusCode();
                var data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<AnalyzeCommentResponse>(data);
                return result;
            }
        }

        private uint? CalculateNudeScore(double doubleScore, IGuildUser user, SocketTextChannel channel)
        {
            var toxList = NudeValue.GetOrAdd(user.GuildId,
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<uint>>>());
            var userValue = toxList.GetOrAdd(user.Id, new ConcurrentDictionary<ulong, LinkedList<uint>>());
            var channelValue = userValue.GetOrAdd(channel.Id, new LinkedList<uint>());
            var score = Convert.ToUInt32(doubleScore) * 100;
            if (channelValue.Count == 20)
            {
                channelValue.RemoveLast();
                channelValue.AddFirst(score);
            }
            else
            {
                channelValue.AddFirst(score);
                return null;
            }

            uint totalScore = 0;

            foreach (var x in channelValue) totalScore = x + totalScore;

            return Convert.ToUInt32(totalScore / userValue.Count);
        }

        private void ClearChannelNudeScore(IGuildUser user, SocketTextChannel channel)
        {
            var toxList = NudeValue.GetOrAdd(user.GuildId,
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<uint>>>());
            var userValue = toxList.GetOrAdd(user.Id, new ConcurrentDictionary<ulong, LinkedList<uint>>());
            var channelValue = userValue.GetOrAdd(channel.Id, new LinkedList<uint>());
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

        private async Task NudeWarn(IGuildUser user, SocketTextChannel channel)
        {
            var guildWarn = WarnAmount.GetOrAdd(user.GuildId, new ConcurrentDictionary<ulong, uint>());
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
                        .Take(100).ToArray().ToList()).ConfigureAwait(false);
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
                        WarnReason.Mute, (await channel.GetMessagesAsync().FlattenAsync())
                        .Where(m => m.Author.Id == user.Id)
                        .Take(100).ToArray().ToList()).ConfigureAwait(false);
                    await _muteService.TimedMute(user, TimeSpan.FromHours(1)).ConfigureAwait(false);
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