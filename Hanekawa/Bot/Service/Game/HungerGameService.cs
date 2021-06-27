using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Bot.Service.ImageGeneration;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.Extensions;
using Hanekawa.HungerGames;
using Hanekawa.HungerGames.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using Quartz;
using Quartz.Util;
using static Disqord.LocalCustomEmoji;
using LogLevel = NLog.LogLevel;

namespace Hanekawa.Bot.Service.Game
{
    public class HungerGameService : DiscordClientService, IJob
    {
        private readonly Hanekawa _bot;
        private readonly HungerGameClient _client;
        private readonly ImageGenerationService _image;
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;

        public HungerGameService(DiscordClientBase clientBase, ILogger<HungerGameService> logger, Hanekawa bot,
            IServiceProvider provider, HungerGameClient client, ImageGenerationService image) : base(logger, clientBase)
        {
            _bot = bot;
            _provider = provider;
            _client = client;
            _image = image;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public Task Execute(IJobExecutionContext context)
        {
            _ = ExecuteAsync();
            return Task.CompletedTask;
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (e.Member.IsBot) return;
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var status = await db.HungerGameStatus.FindAsync(e.GuildId.Value);
                if (status is not {Stage: GameStage.Signup}) return;

                if (!TryParse(status.EmoteMessageFormat, out var result)) return;
                if (e.Emoji.Name != result.Name) return;

                var dbUser = await db.HungerGameProfiles.FindAsync(e.GuildId.Value, e.Member.Id);
                if (dbUser != null) return;

                await db.HungerGameProfiles.AddAsync(new HungerGameProfile
                {
                    GuildId = e.Member.GuildId,
                    UserId = e.Member.Id,
                    Name = e.Member.Nick ?? e.Member.Name,
                    Avatar = e.Member.GetAvatarUrl(CdnAssetFormat.Png),
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
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Hunger Game Service) Crash when adding user - {exception.Message}");
            }
        }

        protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var status = await db.HungerGameStatus.FindAsync(e.GuildId.Value);
                if (status is not {Stage: GameStage.Signup}) return;
                var member = await _bot.GetOrFetchMemberAsync(e.GuildId.Value, e.UserId);
                var dbUser = await db.HungerGameProfiles.FindAsync(e.GuildId.Value, member.Id);
                if (dbUser == null) return;
                db.HungerGameProfiles.Remove(dbUser);
                await db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Hunger Game Service) Crash when removing user - {exception.Message}");
            }
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            try
            {
                if (e.User.IsBot) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var status = await db.HungerGameStatus.FindAsync(e.GuildId);
                if (status == null) return;
                var dbUser = await db.HungerGameProfiles.FindAsync(e.GuildId, e.User.Id);
                if (dbUser == null) return;
                switch (status.Stage)
                {
                    case GameStage.OnGoing:
                        dbUser.Health = 0;
                        dbUser.Alive = false;
                        dbUser.Avatar = _bot.GetGuild(e.GuildId).GetIconUrl(CdnAssetFormat.Png);
                        break;
                    case GameStage.Signup:
                        db.HungerGameProfiles.Remove(dbUser);
                        break;
                    case GameStage.Closed:
                        return;
                }

                await db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Hunger Game Service) Crash when removing user - {exception.Message}");
            }
        }

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var status = await db.HungerGameStatus.FindAsync(e.NewMember.GuildId);
                if (status == null) return;
                var profile =
                    await db.HungerGameProfiles.FindAsync(e.NewMember.GuildId, e.NewMember.Id);
                if (profile == null) return;
                profile.Name = e.NewMember.Nick ?? e.NewMember.Name;
                profile.Avatar = e.NewMember.GetAvatarUrl(CdnAssetFormat.Png);
                await db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"Crash when updating participant avatar or name - {exception.Message}");
            }
        }

        public async Task ExecuteAsync()
        {
            var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            foreach (var x in await db.GuildConfigs.Where(x => x.Premium.HasValue
                                                               && x.Premium.Value > DateTimeOffset.UtcNow)
                .ToArrayAsync())
            {
                var cfg = await db.HungerGameStatus.FindAsync(x.GuildId);
                if (!cfg.EventChannel.HasValue) continue;
                switch (cfg.Stage)
                {
                    case GameStage.Signup:
                        await StartGameAsync(cfg, db);
                        break;
                    case GameStage.OnGoing:
                        await NextRoundAsync(cfg, db);
                        break;
                    case GameStage.Closed:
                        await StartSignUpAsync(cfg, db);
                        break;
                    default:
                        continue;
                }

                await db.SaveChangesAsync();
            }
        }

        private async Task StartSignUpAsync(HungerGameStatus cfg, DbService db)
        {
            if (!TryParse(cfg.EmoteMessageFormat, out var result)) return;
            var channelId = cfg.SignUpChannel ?? cfg.EventChannel.Value;
            if (result == null) return;
            cfg.Stage = GameStage.Signup;
            cfg.SignUpStart = DateTimeOffset.UtcNow;
            var msgContent = "New Hunger Game event has started!\n" +
                             $"To enter, react to this message with {result} !";
            IUserMessage msg;
            try
            {
                msg = await _bot.SendMessageAsync(channelId, new LocalMessage
                {
                    Content = msgContent,
                    Attachments = null,
                    Embeds = null,
                    AllowedMentions = LocalAllowedMentions.None,
                    IsTextToSpeech = false
                });
                await msg.AddReactionAsync(result);
            }
            catch
            {
                cfg.EmoteMessageFormat = "<:Rooree:761209568365248513>";
                TryParse("<:Rooree:761209568365248513>", out result);
                msg = await _bot.SendMessageAsync(channelId, new LocalMessage
                {
                    Content = msgContent,
                    Attachments = null,
                    Embeds = null,
                    AllowedMentions = LocalAllowedMentions.None,
                    IsTextToSpeech = false
                });
                await msg.AddReactionAsync(result);
            }

            await db.SaveChangesAsync();
        }

        private async Task<bool> StartGameAsync(HungerGameStatus cfg, DbService db, bool test = false)
        {
            var cd = cfg.SignUpStart.AddHours(-3);
            if (!test && cd.AddHours(23) >= DateTimeOffset.UtcNow) return false;
            var guild = _bot.GetGuild(cfg.GuildId);
            cfg.Stage = GameStage.OnGoing;
            var participants = await AddDefaultsAsync(db, guild);
            var channelId = cfg.SignUpChannel ?? cfg.EventChannel!.Value;
            var messages = new List<string>();
            var sb = new StringBuilder();
            sb.AppendLine("Sign up is closed and here's the participants for current Hunger Games!");
            for (var i = 0; i < participants.Count;)
            {
                for (var j = 0; j < 5; j++)
                {
                    var x = participants[i];
                    switch (j)
                    {
                        case 4:
                            sb.Append($"**{x.Name}**");
                            break;
                        default:
                            sb.Append($"**{x.Name}** - ");
                            break;
                    }

                    i++;
                }

                sb.AppendLine();
                if (sb.Length < 1800) continue;
                messages.Add(sb.ToString());
                sb.Clear();
            }

            if (sb.Length > 0) messages.Add(sb.ToString());
            var channel = guild.GetChannel(channelId);
            if (channel != null)
            {
                var textChannel = channel as ITextChannel;
                foreach (var x in messages)
                    await textChannel.SendMessageAsync(new LocalMessage
                    {
                        Content = x,
                        Attachments = null,
                        Embeds = null,
                        AllowedMentions = LocalAllowedMentions.None,
                        IsTextToSpeech = false
                    });
            }

            var evtChan = guild.GetChannel(cfg.EventChannel.Value);
            if (evtChan != null)
                await _bot.SendMessageAsync(channelId, new LocalMessage
                {
                    Content = $"Game starts in {(evtChan as CachedTextChannel)?.Mention}",
                    Attachments = null,
                    Embeds = null,
                    AllowedMentions = LocalAllowedMentions.None,
                    IsTextToSpeech = false
                });

            await db.HungerGames.AddAsync(new HungerGame
            {
                Id = Guid.NewGuid(),
                GuildId = cfg.GuildId,
                Alive = participants.Count,
                Participants = participants.Count,
                Round = 0
            });
            await db.SaveChangesAsync();
            var check = await db.HungerGames.FirstOrDefaultAsync(x => x.GuildId == cfg.GuildId);
            if (check == null) return true;
            cfg.GameId = check.Id;
            return true;
        }

        private async Task NextRoundAsync(HungerGameStatus cfg, DbService db)
        {
            if (!cfg.EventChannel.HasValue) return;
            var guild = _bot.GetGuild(cfg.GuildId);
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
            cfg.GameId ??= game.Id;

            // Determine each participant event (alive)
            var result = _client.PlayAsync(participants);
            await SendResultsAsync(cfg, game, result, alive, guild);

            var resultAlive = result.Count(x => x.After.Alive);
            game.Alive = resultAlive;
            game.Round++;
            await db.SaveChangesAsync();
            // Only 1 person alive? Announce and reward
            if (resultAlive > 1) return;
            await RewardWinnerAsync(cfg, db, result, guild, participants, game);
        }

        private async Task SendResultsAsync(HungerGameStatus cfg, HungerGame game, List<UserAction> result, int alive,
            CachedGuild guild)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"**Hunger Game Round {game.Round + 1}!**");
            var messages = new List<string>();

            // Create text messages
            foreach (var msg in from x in result
                    where x.Before.Alive
                    where !x.Message.IsNullOrWhiteSpace()
                    select $"**{x.After.Name}**: {x.Message}")
                // var msg = !x.AfterProfile.Bot 
                //     ? $"**{guild.GetMember(x.AfterProfile.UserId).DisplayName ?? "User Left Server"}**: {x.Message}" 
                // : $"**{x.AfterProfile.Name}**: {x.Message}";
                if (sb.Length + msg.Length >= 2000)
                {
                    messages.Add(sb.ToString());
                    sb.Clear();
                    sb.AppendLine(msg);
                }
                else
                {
                    sb.AppendLine(msg);
                }

            if (sb.Length > 0) messages.Add(sb.ToString());

            // Generate banners
            var tempPart = result.ToList();

            var imgCount = Math.Ceiling((double) alive / 25);
            if (imgCount <= 0) imgCount = 1;
            var channel = guild.GetChannel(cfg.EventChannel.Value) as CachedTextChannel;

            // Make and send images
            await CreateAndSendImagesAsync(imgCount, tempPart, channel, guild);

            // Send Text
            foreach (var t in messages)
                try
                {
                    await channel.SendMessageAsync(new LocalMessage
                    {
                        Content = t,
                        Attachments = null,
                        Embeds = null,
                        AllowedMentions = LocalAllowedMentions.None,
                        IsTextToSpeech = false
                    });
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, e, e.Message);
                }
        }

        private async Task RewardWinnerAsync(HungerGameStatus cfg, DbService db, List<UserAction> result,
            CachedGuild guild, List<HungerGameProfile> participants,
            HungerGame game)
        {
            IMember user = null;
            var winner = result.FirstOrDefault(x => x.After.Alive);
            if (winner != null && !winner.After.Bot)
            {
                user = await guild.GetOrFetchMemberAsync(winner.After.UserId);
                var userData = await db.GetOrCreateUserData(winner.After.GuildId, winner.After.UserId);
                userData.Exp += cfg.ExpReward;
                userData.Credit += cfg.CreditReward;
                userData.CreditSpecial += cfg.SpecialCreditReward;
            }

            if (!cfg.SignUpChannel.HasValue)
            {
                await db.SaveChangesAsync();
                return;
            }

            var announce = guild.GetChannel(cfg.SignUpChannel.Value) as CachedTextChannel;
            var sb = new StringBuilder();
            if (user == null && !winner.After.Bot)
            {
                sb.AppendLine("Couldn't find the winner soooo... new Hunger Game soon !");
            }
            else if (winner.After.Bot)
            {
                sb.AppendLine(
                    $"{winner.After.Name} is the new Hunger Game Champion, unfortently its a bot so no rewards!");
            }
            else
            {
                var role = await RewardRoleAsync(cfg, user as CachedMember);
                sb.AppendLine($"{user.Mention} is the new Hunger Game Champion!");
                sb.AppendLine("They have been rewarded with the following:");
                var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(guild.Id);
                if (cfg.ExpReward > 0) sb.AppendLine($"{cfg.ExpReward} exp");
                if (cfg.CreditReward > 0)
                    sb.AppendLine($"{currencyCfg.CurrencyName}: {currencyCfg.ToCurrencyFormat(cfg.CreditReward)}");
                if (cfg.SpecialCreditReward > 0)
                    sb.AppendLine(
                        $"{currencyCfg.SpecialCurrencyName}: {currencyCfg.ToCurrencyFormat(cfg.SpecialCreditReward, true)}");
                if (role != null) sb.AppendLine($"{role.Mention} role");
            }

            await announce.SendMessageAsync(new LocalMessage
            {
                Content = sb.ToString(),
                Attachments = null,
                Embeds = null,
                AllowedMentions = LocalAllowedMentions.None,
                IsTextToSpeech = false
            });
            try
            {
                await db.HungerGameHistories.AddAsync(new HungerGameHistory
                {
                    GameId = cfg.GameId.Value,
                    GuildId = cfg.GuildId,
                    Winner = winner.After.UserId,
                    CreditReward = cfg.CreditReward,
                    SpecialCreditReward = cfg.SpecialCreditReward,
                    ExpReward = cfg.ExpReward
                });
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, $"(Hunger Game Service) Couldn't add game history - {e.Message}");
            }

            db.HungerGameProfiles.RemoveRange(participants);
            db.HungerGames.Remove(game);
            cfg.Stage = GameStage.Closed;
            cfg.GameId = null;
            await db.SaveChangesAsync();
        }

        private async Task CreateAndSendImagesAsync(double imgCount, List<UserAction> tempPart, IMessageChannel channel,
            IGuild guild)
        {
            var attachments = new List<LocalAttachment>();
            for (var i = 0; i < imgCount; i++)
            {
                var toTake = tempPart.Count >= 25 ? 25 : tempPart.Count;
                var amount = tempPart.Take(toTake).OrderByDescending(x => x.Before.Alive).ToList();
                tempPart.RemoveRange(0, toTake);
                var image = await _image.GenerateEventImageAsync(guild, tempPart, amount.Count);
                attachments.Add(new LocalAttachment(image, "HungerGame.png"));
            }

            await channel.SendMessageAsync(new LocalMessage
            {
                Attachments = attachments,
                Content = null,
                Embeds = null,
                AllowedMentions = LocalAllowedMentions.None,
                Reference = null,
                IsTextToSpeech = false
            });
        }

        private async Task<IRole> RewardRoleAsync(HungerGameStatus cfg, IMember winner)
        {
            if (!cfg.RoleReward.HasValue) return null;
            var guild = _bot.GetGuild(winner.GuildId);
            if (!guild.Roles.TryGetValue(cfg.RoleReward.Value, out var role)) return null;
            var toRemove = guild.Members.Where(x => x.Value.GetRoles().ContainsKey(role.Id)).ToList();
            foreach (var x in toRemove)
                try
                {
                    await x.Value.RevokeRoleAsync(role.Id);
                }
                catch
                {
                    /* IGNORE */
                }

            try
            {
                await winner.GrantRoleAsync(role.Id);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, $"(Hunger Game Service) Couldn't grant winner role - {e.Message}");
                return null;
            }

            return role;
        }

        private static async Task<List<HungerGameProfile>> AddBoostersAsync(DbService db, CachedGuild guild)
        {
            var toReturn = new List<HungerGameProfile>();
            foreach (var (_, user) in guild.Members.Where(x => x.Value.BoostedAt.HasValue).ToList())
            {
                var check = await db.HungerGameProfiles.FindAsync(guild.Id, user.Id);
                if (check != null) continue;
                toReturn.Add(new HungerGameProfile
                {
                    GuildId = user.GuildId,
                    UserId = user.Id,
                    Name = user.Name,
                    Avatar = user.GetAvatarUrl(CdnAssetFormat.Png),
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
            }

            return toReturn;
        }

        private static async Task<List<HungerGameProfile>> AddDefaultsAsync(DbService db, CachedGuild guild)
        {
            int toAddNumber;
            var profiles = await db.HungerGameProfiles.Where(x => x.GuildId == guild.Id).ToListAsync();
            var boosters = await AddBoostersAsync(db, guild);
            profiles.AddRange(boosters);
            await db.HungerGameProfiles.AddRangeAsync(boosters);
            switch (profiles.Count)
            {
                case 0:
                    toAddNumber = 25;
                    break;
                case <= 25:
                    toAddNumber = 25 - profiles.Count;
                    break;
                default:
                {
                    toAddNumber = Convert.ToInt32(25 * Math.Ceiling(Convert.ToDouble(profiles.Count / 25))) -
                                  profiles.Count;
                    if (toAddNumber < 0)
                        toAddNumber = Convert.ToInt32(25 * Math.Ceiling(Convert.ToDouble((profiles.Count + 13) / 25))) -
                                      profiles.Count;
                    break;
                }
            }

            var defaults = await db.HungerGameDefaults.Take(toAddNumber).ToListAsync();
            var toAdd = defaults.Select(x => new HungerGameProfile
                {
                    GuildId = guild.Id,
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
                })
                .ToList();

            profiles.AddRange(toAdd);
            await db.HungerGameProfiles.AddRangeAsync(toAdd);
            await db.SaveChangesAsync();
            return profiles;
        }
    }
}