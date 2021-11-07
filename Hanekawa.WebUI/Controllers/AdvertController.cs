using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Infrastructure;
using Hanekawa.Infrastructure.Extensions;
using Hanekawa.Entities;
using Hanekawa.Entities.Account;
using Hanekawa.Entities.Advertise;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.Entities.Giveaway;
using Hanekawa.WebUI.Extensions;
using Hanekawa.WebUI.Bot.Service.Cache;
using Hanekawa.WebUI.Bot.Service.Experience;
using Hanekawa.WebUI.Models;
using Hanekawa.WebUI.Utility;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using Quartz.Util;

namespace Hanekawa.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvertController : ControllerBase
    {
        private readonly CacheService _cache;
        private readonly Bot.Hanekawa _client;
        private readonly DbService _db;
        private readonly ExpService _exp;
        private readonly Logger _log;

        public AdvertController(DbService db, Bot.Hanekawa client, CacheService cache, ExpService exp)
        {
            _db = db;
            _client = client;
            _log = LogManager.GetCurrentClassLogger();
            _cache = cache;
            _exp = exp;
        }

        [HttpPost("dbl")]
        public async Task<IActionResult> Dsl([FromBody] DslWebhook model, CancellationToken token)
        {
            try
            {
                // Check if user has a authorization in the header, else return forbidden
                // we only accept requests with an authorization in the header
                if (!Request.Headers.TryGetValue("Authorization", out var authCode))
                    return Unauthorized("No authorization header");
                var guildId = Convert.ToUInt64(model.Guild);
                var cfg = await _db.Top.FindAsync(new object[] {guildId}, token); // Get the key from database
                // If there's no config, the guild doesn't have it enabled
                if (cfg == null) return BadRequest();
                // Make sure the key is correct
                if (cfg.AuthKey != authCode.ToString()) return Unauthorized("Invalid key");

                var guild = _client.GetGuild(guildId); // Get guild and check if bot is in guild, could be kicked
                if (guild == null) return BadRequest("Invalid guild");
                // Get user data and reward from config
                var userId = Convert.ToUInt64(model.User);
                var userData = await _db.GetOrCreateEntityAsync<Account>(guildId, userId);
                var user = await guild.FetchMemberAsync(userId);
                if (cfg.SpecialCredit > 0)
                    userData.CreditSpecial +=
                        cfg.SpecialCredit; // Manually add as AddExp doesn't do special credit, maybe add later?
                if (user != null)
                {
                    await _exp.AddExpAsync(user, userData, cfg.ExpGain, cfg.CreditGain, _db, ExpSource.Other);
                    if (cfg.RoleIdReward.HasValue &&
                        !user.GetRoles()
                            .ContainsKey(cfg.RoleIdReward
                                .Value)) // Reward a role if its in the config and the user doesn't already have it
                        await user.GrantRoleAsync(cfg.RoleIdReward.Value);
                }
                else
                {
                    if (cfg.ExpGain > 0) userData.Exp += cfg.ExpGain;
                    if (cfg.ExpGain > 0) userData.TotalExp += cfg.ExpGain;
                    if (cfg.CreditGain > 0) userData.Credit += cfg.CreditGain;
                }

                // Add a log entry for the vote to keep track of votes
                await _db.VoteLogs.AddAsync(new VoteLog
                {
                    GuildId = guildId,
                    UserId = userId,
                    Type = model.Type,
                    Time = DateTimeOffset.UtcNow
                }, token);

                var logCfg = await _db.GetOrCreateEntityAsync<LoggingConfig>(guild.Id);
                var logChannel = guild.GetChannel(logCfg.LogAvi.Value);
                if (logChannel != null)
                {
                    var name = $"{user}";
                    if (name.IsNullOrWhiteSpace()) name = $"{userId}";
                    var embed = new LocalEmbed
                    {
                        Title = "Top.gg Vote!",
                        Color = _cache.GetColor(guild.Id),
                        Description = $"{name} just voted for the server!",
                        Footer = new LocalEmbedFooter
                            {IconUrl = user?.GetAvatarUrl(), Text = $"Username: {name} ({userId})"}
                    };
                    await _client.SendMessageAsync(logChannel.Id, new LocalMessage
                    {
                        Embeds = new[] {embed},
                        Attachments = null,
                        Content = null,
                        AllowedMentions = LocalAllowedMentions.None,
                        Reference = null,
                        IsTextToSpeech = false
                    });
                }
                
                _log.Log(LogLevel.Info,
                    $"(Advert Endpoint) Rewarded {userId} in {guild.Id} for voting on the server!");
                
                var giveaways = await _db.Giveaways
                    .Where(x => x.GuildId == guildId && x.Type == GiveawayType.Vote && x.Active).ToListAsync(token);
                var sb = new StringBuilder();
                if (giveaways.Count > 0 && user != null)
                {
                    sb.AppendLine("Your entry has been registered toward the following giveaways:");
                    var length = sb.Length;
                    foreach (var x in giveaways) await GiveawayCheck(x, user, sb, userData, guildId, userId);

                    if (sb.Length == length) sb.Clear();
                }

                await _db.SaveChangesAsync(token);
                if (cfg.Message.IsNullOrWhiteSpace() && user == null)
                    return Accepted(); // Check if there's a message to be sent, else we good
                try
                {
                    var str = new StringBuilder();
                    var currencyCfg = await _db.GetOrCreateEntityAsync<CurrencyConfig>(guildId);
                    if (cfg.ExpGain > 0) str.AppendLine($"{cfg.ExpGain} Exp");
                    if (cfg.CreditGain > 0)
                        str.AppendLine($"{currencyCfg.CurrencyName}: {currencyCfg.ToCurrencyFormat(cfg.CreditGain)}");
                    if (cfg.SpecialCredit > 0)
                        str.AppendLine(
                            $"{currencyCfg.SpecialCurrencyName}: {currencyCfg.ToCurrencyFormat(cfg.SpecialCredit, true)}");
                    var dmChannel = await user.CreateDirectChannelAsync();
                    await _client.SendMessageAsync(dmChannel.Id, new LocalMessage
                    {
                        Embeds = new[]
                        {
                            new LocalEmbed
                            {
                                Description = $"{MessageUtil.FormatMessage(cfg.Message, user, guild)}\n" +
                                              "You've been rewarded:\n" +
                                              $"{str}\n{sb}",
                                Color = _cache.GetColor(guild.Id)
                            }
                        },
                        AllowedMentions = LocalAllowedMentions.None,
                        Attachments = null,
                        Content = null,
                        Reference = null,
                        IsTextToSpeech = false
                    });
                }
                catch
                {
                    // Ignore, the user likely has closed DM or blocked the bot.
                }

                return Accepted();
            }
            catch (Exception e)
            {
                _log.Log(LogLevel.Error, e, $"(Advert Endpoint) Error in awarding user for voting - {e.Message}");
                return StatusCode(500);
            }
        }

        private async Task GiveawayCheck(Giveaway x, IMember user, StringBuilder sb, Account userData, ulong guildId,
            ulong userId)
        {
            if (!x.Active) return;
            if (x.CloseAtOffset.HasValue && x.CloseAtOffset.Value <= DateTimeOffset.UtcNow) return;
            if (x.ServerAgeRequirement.HasValue &&
                user.JoinedAt.Value.Add(x.ServerAgeRequirement.Value) > DateTimeOffset.UtcNow)
            {
                sb.AppendLine(
                    $"You don't qualify for {x.Name} giveaway, your account has to be in the server for at least {x.ServerAgeRequirement.Value.Humanize()}");
                return;
            }

            if (userData.Level < x.LevelRequirement)
            {
                sb.AppendLine(
                    $"You don't qualify for {x.Name} giveaway, you need to be at least of level{x.LevelRequirement} to enter.");
                return;
            }

            await _db.GiveawayParticipants.AddAsync(new GiveawayParticipant
            {
                Id = Guid.NewGuid(),
                GuildId = guildId,
                UserId = userId,
                GiveawayId = x.Id,
                Giveaway = x,
                Entry = DateTimeOffset.UtcNow
            });
            sb.AppendLine($"{x.Name}");
        }
    }
}