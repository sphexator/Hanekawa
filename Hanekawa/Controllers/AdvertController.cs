﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Services;
using Hanekawa.Bot.Services.Economy;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Advertise;
using Hanekawa.Database.Tables.Giveaway;
using Hanekawa.Extensions;
using Hanekawa.Models;
using Hanekawa.Shared;
using Hanekawa.Shared.Command;
using Hanekawa.Utility;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using Quartz.Util;

namespace Hanekawa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvertController : ControllerBase
    {
        private readonly DbService _db;
        private readonly Bot.Hanekawa _client;
        private readonly ExpService _exp;
        private readonly NLog.Logger _log;
        private readonly ColourService _colour;
        private readonly CurrencyService _currency;

        public AdvertController(DbService db, Bot.Hanekawa client, ExpService exp, ColourService colour, CurrencyService currency)
        {
            _db = db;
            _client = client;
            _exp = exp;
            _log = LogManager.GetCurrentClassLogger();
            _colour = colour;
            _currency = currency;
        }

        [HttpPost("dbl")]
        public async Task<IActionResult> Dsl([FromBody] DslWebhook model)
        {
            try
            {
                // Check if header has right agent
                if (!Request.Headers.TryGetValue("User-Agent", out var agent)) return BadRequest();
                if (!agent.Contains("DBL")) return BadRequest(); // If not send bad request, only accepting DBL user agents
                                                                 // Check if user has a authorization in the header, else return forbidden
                                                                 // we only accept requests with an authorization in the header
                if (!Request.Headers.TryGetValue("Authorization", out var authCode)) return Unauthorized("No authorization header");
                var guildId = Convert.ToUInt64(model.Guild);
                var cfg = await _db.DblAuths.FindAsync(guildId); // Get the key from database
                                                                 // If there's no config, the guild doesn't have it enabled
                if (cfg == null) return BadRequest();
                // Make sure the key is correct
                if (cfg.AuthKey.ToString() != authCode.ToString()) return Unauthorized("Invalid key");

                var guild = _client.GetGuild(guildId); // Get guild and check if bot is in guild, could be kicked
                if (guild == null) return BadRequest("Invalid guild");
                // Get user data and reward from config
                var userId = Convert.ToUInt64(model.User);
                var userData = await _db.GetOrCreateUserData(guildId, userId);
                var user = await guild.GetOrFetchMemberAsync(userId) as CachedMember;
                if (cfg.SpecialCredit > 0) userData.CreditSpecial += cfg.SpecialCredit; // Manually add as AddExp doesn't do special credit, maybe add later?
                if (user != null)
                {
                    await _exp.AddExpAsync(user, userData, cfg.ExpGain, cfg.CreditGain, _db);
                    if (cfg.RoleIdReward.HasValue && !user.Roles.ContainsKey(cfg.RoleIdReward.Value)) // Reward a role if its in the config and the user doesn't already have it
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
                });

                var logCfg = await _db.GetOrCreateLoggingConfigAsync(guild);
                if (logCfg.LogAvi.HasValue)
                {
                    var name = $"{user}";
                    if (name.IsNullOrWhiteSpace()) name = $"{userId}";
                    await guild.GetTextChannel(logCfg.LogAvi.Value).SendMessageAsync(null, false, new LocalEmbedBuilder
                    {
                        Title = "Top.gg Vote!",
                        Color = _colour.Get(guild.Id.RawValue),
                        Description = $"{name} just voted for the server!",
                        Footer = new LocalEmbedFooterBuilder{ IconUrl = user?.GetAvatarUrl(), Text = $"Username: {name} ({userId})"}
                    }.Build());
                }
                _log.Log(LogLevel.Info, $"(Advert Endpoint) Rewarded {userId} in {guild.Id.RawValue} for voting on the server!");

                var giveaways = await _db.Giveaways
                    .Where(x => x.GuildId == guildId && x.Type == GiveawayType.Vote && x.Active).ToListAsync();
                var sb = new StringBuilder();
                if (giveaways.Count > 0 && user != null)
                {
                    sb.AppendLine("Your entry has been registered toward the following giveaways:");
                    var length = sb.Length;
                    for (var i = 0; i < giveaways.Count; i++)
                    {
                        var x = giveaways[i];
                        if(!x.Active) continue;
                        if(x.CloseAtOffset.HasValue && x.CloseAtOffset.Value <= DateTimeOffset.UtcNow) continue;
                        if (x.ServerAgeRequirement.HasValue &&
                            user.JoinedAt.Add(x.ServerAgeRequirement.Value) > DateTimeOffset.UtcNow)
                        {
                            sb.AppendLine(
                                $"You don't qualify for {x.Name} giveaway, your account has to be in the server for at least {x.ServerAgeRequirement.Value.Humanize()}");
                            continue;
                        }

                        if (userData.Level < x.LevelRequirement)
                        {
                            sb.AppendLine(
                                $"You don't qualify for {x.Name} giveaway, you need to be at least of level{x.LevelRequirement} to enter.");
                            continue;
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

                    if (sb.Length == length) sb.Clear();
                }

                await _db.SaveChangesAsync();
                if (cfg.Message.IsNullOrWhiteSpace() && user == null) return Accepted(); // Check if there's a message to be sent, else we good
                try
                {
                    var str = new StringBuilder();
                    var currencyCfg = await _db.GetOrCreateCurrencyConfigAsync(guildId);
                    if (cfg.ExpGain > 0) str.AppendLine($"{cfg.ExpGain} Exp");
                    if (cfg.CreditGain > 0) str.AppendLine($"{currencyCfg.CurrencyName}: {_currency.ToCurrency(currencyCfg, cfg.CreditGain)}");
                    if (cfg.SpecialCredit > 0) str.AppendLine($"{currencyCfg.SpecialCurrencyName}: {_currency.ToCurrency(currencyCfg, cfg.SpecialCredit, true)}");
                    if (user.DmChannel != null) // determine if dm channel is already created, else create it and send message
                        await user.DmChannel.SendMessageAsync(
                            $"{MessageUtil.FormatMessage(cfg.Message, user, user.Guild)}\n" +
                            "You've been rewarded:\n" +
                            $"{str}\n{sb}", false,
                            null,
                            LocalMentions.None);
                    else
                    {
                        var channel = await user.CreateDmChannelAsync();
                        await channel.SendMessageAsync($"{MessageUtil.FormatMessage(cfg.Message, user, user.Guild)}\n" +
                                                       "You've been rewarded:\n" +
                                                       $"{str}\n{sb}", false,
                            null,
                            LocalMentions.None);
                    }
                }
                catch
                {
                    // Ignore, the user likely has closed DMs
                }
                return Accepted();
            }
            catch (Exception e)
            {
                _log.Log(NLog.LogLevel.Error, e, $"(Advert Endpoint) Error in awarding user for voting - {e.Message}");
                return StatusCode(500);
            }
        }
    }
}