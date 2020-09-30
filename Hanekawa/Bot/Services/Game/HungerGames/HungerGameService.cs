﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Bot.Services.Economy;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.Shared.Game.HungerGame;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Util;

namespace Hanekawa.Bot.Services.Game.HungerGames
{
    public class HungerGameService : INService, IRequired, IJob
    {
        private readonly Hanekawa _client;
        private readonly InternalLogService _log;
        private readonly CurrencyService _currency;
        private readonly ImageGenerator _image;
        private readonly HungerGame _game;
        private readonly IServiceProvider _provider;

        public HungerGameService(Hanekawa client, IServiceProvider provider, ImageGenerator image, HungerGame game, CurrencyService currency, InternalLogService log)
        {
            _client = client;
            _provider = provider;
            _image = image;
            _game = game;
            _currency = currency;
            _log = log;

            _client.ReactionAdded += AddParticipant;
            _client.ReactionRemoved += RemoveParticipant;
            _client.MemberLeft += RemoveParticipant;
        }

        private Task AddParticipant(ReactionAddedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!e.User.HasValue) return;
                if (e.User.Value.IsBot) return;
                if (!(e.User.Value is CachedMember user)) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var status = await db.HungerGameStatus.FindAsync(user.Guild.Id.RawValue);
                if (status == null) return;
                if (status.Stage != HungerGameStage.Signup) return;
                var dbUser = await db.HungerGameProfiles.FindAsync(user.Guild.Id.RawValue, user.Id.RawValue);
                if (dbUser != null) return;
                await db.HungerGameProfiles.AddAsync(new HungerGameProfile
                {
                    GuildId = user.Guild.Id.RawValue,
                    UserId = user.Id.RawValue,
                    Name = null,
                    Avatar = null,
                    Bot = false,
                    Alive = true,
                    Health = 100,
                    Stamina = 100,
                    Bleeding = false,
                    Hunger = 100,
                    Thirst = 100,
                    Tiredness = 0,
                    Move = 0,
                    Water = 0,
                    Bullets = 0,
                    FirstAid = 0,
                    Food = 0,
                    MeleeWeapon = 0,
                    RangeWeapon = 0,
                    Weapons = 0
                });
                await db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private Task RemoveParticipant(MemberLeftEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.User.IsBot) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var status = await db.HungerGameStatus.FindAsync(e.Guild.Id.RawValue);
                if (status == null) return;
                if (status.Stage != HungerGameStage.Signup) return;
                var dbUser = await db.HungerGameProfiles.FindAsync(e.Guild.Id.RawValue, e.User.Id.RawValue);
                if (dbUser == null) return;
                db.HungerGameProfiles.Remove(dbUser);
                await db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private Task RemoveParticipant(ReactionRemovedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!e.User.HasValue) return;
                if (e.User.Value.IsBot) return;
                if (!(e.User.Value is CachedMember user)) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var status = await db.HungerGameStatus.FindAsync(user.Guild.Id.RawValue);
                if (status == null) return;
                if (status.Stage != HungerGameStage.Signup) return;
                var dbUser = await db.HungerGameProfiles.FindAsync(user.Guild.Id.RawValue, user.Id.RawValue);
                if (dbUser == null) return;
                db.HungerGameProfiles.Remove(dbUser);
                await db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _ = Task.Run(async () =>
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var list = await db.GuildConfigs.Where(x => x.Premium).ToListAsync();
                foreach (var x in list)
                {
                    var cfg = await db.HungerGameStatus.FindAsync(x.GuildId);
                    if (cfg == null) continue;
                    if (!cfg.EventChannel.HasValue) continue;
                    if (!cfg.SignUpChannel.HasValue) continue;
                    switch (cfg.Stage)
                    {
                        case HungerGameStage.Signup:
                            await StartGameAsync(cfg, db);
                            break;
                        case HungerGameStage.OnGoing:
                            await NextRoundAsync(cfg, db);
                            break;
                        case HungerGameStage.Closed:
                            await StartSignUpAsync(cfg, db);
                            break;
                        default:
                            continue;
                    }

                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        public async Task StartSignUpAsync(HungerGameStatus cfg, DbService db)
        {
            if (!LocalCustomEmoji.TryParse(cfg.EmoteMessageFormat, out var result)) return;
            if (!cfg.SignUpChannel.HasValue) return;
            cfg.Stage = HungerGameStage.Signup;
            await _client.GetGuild(cfg.GuildId).GetTextChannel(cfg.SignUpChannel.Value).SendMessageAsync(
                "New Hunger Game event has started!\n" +
                $"To enter, react to this message with {result} !");
            await db.SaveChangesAsync();
        }

        public async Task<bool> StartGameAsync(HungerGameStatus cfg, DbService db, DateTimeOffset? cd = null)
        {
            if(cd == null) cd = cfg.SignUpStart.AddHours(23);
            if (cd >= DateTimeOffset.UtcNow) return false;
            cfg.Stage = HungerGameStage.OnGoing;
            var participants = await AddDefaultUsers(db, cfg.GuildId);
            if (cfg.SignUpChannel.HasValue)
            {
                var messages = new List<string>();
                var sb = new StringBuilder();
                sb.AppendLine("Sign up is closed and here's the participants for current Hunger Games!");
                for (var i = 0; i < participants.Count;)
                {
                    for (var j = 0; j < 5; j++)
                    {
                        var x = participants[i];
                        var name = x.Bot ? x.Name : _client.GetUser(x.UserId).Mention;
                        if (name.IsNullOrWhiteSpace()) name = x.Name;
                        switch (j)
                        {
                            case 0:
                                sb.Append($"{name} ");
                                break;
                            case 4:
                                sb.Append(name);
                                break;
                            default:
                                sb.Append($"{name} - ");
                                break;
                        }

                        sb.AppendLine();
                        i++;
                    }

                    if (sb.Length < 1800) continue;
                    messages.Add(sb.ToString());
                    sb.Clear();
                }

                var guild = _client.GetGuild(cfg.GuildId);
                var channel = guild.GetTextChannel(cfg.SignUpChannel.Value);
                if (channel != null)
                {
                    for (var i = 0; i < messages.Count; i++)
                    {
                        var x = messages[i];
                        await channel.SendMessageAsync(x, false, null, LocalMentions.None);
                    }
                }

                if (cfg.EventChannel.HasValue && channel != null)
                {
                    var evtChan = guild.GetTextChannel(cfg.EventChannel.Value);
                    if (evtChan != null) await channel.SendMessageAsync($"Game starts in {evtChan.Mention}", false, null, LocalMentions.None);
                }
            }

            await db.HungerGames.AddAsync(new Database.Tables.Account.HungerGame.HungerGame
            {
                Id = new Guid(),
                GuildId = cfg.GuildId,
                Alive = participants.Count,
                Participants = participants.Count,
                Round = 0
            });
            await db.SaveChangesAsync();
            return true;
        }

        public async Task NextRoundAsync(HungerGameStatus cfg, DbService db)
        {
            var guild = _client.GetGuild(cfg.GuildId);
            if (guild == null) return;
            // Total participants
            var participants = await db.HungerGameProfiles
                .Where(x => x.GuildId == cfg.GuildId)
                .OrderByDescending(x => x.Alive)
                .ThenBy(x => x.Bot)
                .ThenBy(x => x.UserId)
                .ToListAsync();
            var game = await db.HungerGames.FirstOrDefaultAsync(x => x.GuildId == cfg.GuildId);
            var alive = participants.Count(x => x.Alive);
            
            // Determine each participant event (alive)
            var result = _game.PlayAsync(participants);
            await db.SaveChangesAsync();
            if (!cfg.EventChannel.HasValue) return;
            
            var sb = new StringBuilder();
            sb.AppendLine($"**Hunger Game Round {game.Round + 1}!**");
            var messages = new List<string>();
            
            // Create text messages
            for (var i = 0; i < result.Count; i++)
            {
                var x = result[i];
                if(!x.BeforeProfile.Alive) continue;
                if(x.Message.IsNullOrWhiteSpace()) continue;
                var msg = $"{guild.GetMember(x.AfterProfile.UserId).Mention ?? "User Left Server"}: {x.Message}";
                if (sb.Length + msg.Length >= 2000)
                {
                    messages.Add(sb.ToString());
                    sb.Clear();
                    sb.AppendLine(msg);
                }
                else sb.AppendLine(msg);
            }

            // Generate banners
            var tempPart = result.ToList();
            var images = new List<Stream>();
            for (var i = 0; i < Math.Ceiling((double)(alive / 25)); i++)
            {
                var toTake = tempPart.Count >= 25 ? 25 : tempPart.Count;
                var amount = tempPart.Take(toTake).ToList();
                tempPart.RemoveRange(0, toTake);
                images.Add(await _image.GenerateEventImageAsync(amount, amount.Count(x => x.AfterProfile.Alive)));
            }
            
            // Send Images
            var channel = guild.GetTextChannel(cfg.EventChannel.Value);
            for (var i = 0; i < images.Count; i++)
            {
                await channel.SendMessageAsync(new LocalAttachment(images[i], "HungerGame.png", false), null, false,
                    null, LocalMentions.None);
            }
            // Send Text
            for (var i = 0; i < messages.Count; i++)
            {
                await channel.SendMessageAsync(messages[i], false, null, LocalMentions.None);
            }
            var resultAlive = result.Count(x => x.AfterProfile.Alive);
            game.Alive = resultAlive;
            game.Round++;
            await db.SaveChangesAsync();
            // Only 1 person alive? Announce and reward
            if (resultAlive > 1) return;
            CachedMember user = null;
            var winner = result.FirstOrDefault(x => x.AfterProfile.Alive);
            if (winner != null)
            {
                user = guild.GetMember(winner.AfterProfile.UserId); 
                var userData = await db.GetOrCreateUserData(winner.AfterProfile.GuildId, winner.AfterProfile.UserId);
                userData.Exp += cfg.ExpReward;
                userData.Credit += cfg.CreditReward;
                userData.CreditSpecial += cfg.SpecialCreditReward;
            }

            if (!cfg.SignUpChannel.HasValue)
            {
                await db.SaveChangesAsync();
                return;
            }
            var announce = guild.GetTextChannel(cfg.SignUpChannel.Value);
            var stringBuilder = new StringBuilder();
            if (user == null)
            {
                stringBuilder.AppendLine("Couldn't find the winner soooo... new Hunger Game soon !");
            }
            else
            {
                var role = await RewardRole(cfg, user);
                stringBuilder.AppendLine($"{user.Mention} is the new Hunger Game Champion!");
                stringBuilder.AppendLine("They have been rewarded with the following:");
                if (cfg.ExpReward > 0) stringBuilder.AppendLine($"{cfg.ExpReward} exp");
                if (cfg.CreditReward > 0) stringBuilder.AppendLine(await _currency.ToCurrency(db, cfg.GuildId, cfg.CreditReward));
                if (cfg.SpecialCreditReward > 0) stringBuilder.AppendLine(await _currency.ToCurrency(db, cfg.GuildId, cfg.SpecialCreditReward, true));
                if (role != null) stringBuilder.AppendLine($"{role.Mention} role");
            }
            await announce.SendMessageAsync(stringBuilder.ToString(), false, null, LocalMentions.None);
            await db.HungerGameHistories.AddAsync(new HungerGameHistory
            {
                GameId = cfg.GameId.Value,
                GuildId = cfg.GuildId,
                Winner = winner.AfterProfile.UserId,
                CreditReward = cfg.CreditReward,
                SpecialCreditReward = cfg.SpecialCreditReward,
                ExpReward = cfg.ExpReward
            });
            db.HungerGameProfiles.RemoveRange(participants);
            db.HungerGames.Remove(game);
            cfg.Stage = HungerGameStage.Closed;
            cfg.GameId = null;
            await db.SaveChangesAsync();
        }

        private async Task<CachedRole> RewardRole(HungerGameStatus cfg, CachedMember winner)
        {
            if (!cfg.RoleReward.HasValue) return null;
            var role = winner.Guild.GetRole(cfg.RoleReward.Value);
            if (role == null) return null;
            var toRemove = winner.Guild.Members.Where(x => x.Value.Roles.ContainsKey(role.Id)).ToList();
            foreach (var x in toRemove)
            {
                try
                {
                    await x.Value.RevokeRoleAsync(role.Id);
                }
                catch { /* IGNORE */}
            }

            try
            {
                await winner.GrantRoleAsync(role.Id);
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Hunger Game Service) Couldn't grant winner role - {e.Message}");
                return null;
            }
            return role;
        }

        private async Task<List<HungerGameProfile>> AddDefaultUsers(DbService db, ulong guildId)
        {
            var profiles = await db.HungerGameProfiles.Where(x => x.GuildId == guildId).ToListAsync();
            var toAdd = (Convert.ToInt32(25 * Math.Ceiling((double) (profiles.Count / 25))) - profiles.Count);
            if (toAdd < 0)
                toAdd = Convert.ToInt32(25 * Math.Ceiling((double) ((profiles.Count + 13) / 25))) - profiles.Count;
            var defaults = await db.HungerGameDefaults.Take(toAdd).ToListAsync();
            var toAddefaults = new List<HungerGameProfile>();
            for (var i = 0; i < defaults.Count; i++)
            {
                var x = defaults[i];
                toAddefaults.Add(new HungerGameProfile
                {
                    GuildId = guildId,
                    UserId = x.Id,
                    Name = x.Name,
                    Avatar = x.Avatar,
                    Bot = true,
                    Alive = true,
                    Health = 50,
                    Stamina = 100,
                    Bleeding = false,
                    Hunger = 100,
                    Thirst = 100,
                    Tiredness = 0,
                    Move = 0,
                    Water = 0,
                    Bullets = 0,
                    FirstAid = 0,
                    Food = 0,
                    MeleeWeapon = 0,
                    RangeWeapon = 0,
                    Weapons = 0
                });
            }

            profiles.AddRange(toAddefaults);
            await db.HungerGameProfiles.AddRangeAsync(toAddefaults);
            await db.SaveChangesAsync();
            return profiles;
        }
    }
}