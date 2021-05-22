using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Hanekawa.Bot.Service.Achievements;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Drop;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Giveaway;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using Quartz;
using LogLevel = NLog.LogLevel;

namespace Hanekawa.Bot.Service.Experience
{
    public partial class ExpService : DiscordClientService, IJob
    {
        private readonly CacheService _cache;
        private readonly AchievementService _achievement;
        private readonly Logger _logger;
        private readonly Hanekawa _bot;
        private readonly IServiceProvider _provider;
        private readonly Random _random;

        public ExpService(CacheService cache, Hanekawa bot, IServiceProvider provider, Random random, AchievementService achievement, ILogger<ExpService> logger) : base(logger, bot)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _cache = cache;
            _bot = bot;
            _provider = provider;
            _random = random;
            _achievement = achievement;

            using var scope = _provider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            foreach (var x in db.LevelExpEvents.ToArray())
            {
                if (x.Time <= DateTime.UtcNow)
                {
                    db.LevelExpEvents.Remove(x);
                    continue;
                }
                var timer = new Timer(async _ =>
                {
                    using var serviceScope = _provider.CreateScope();
                    await using var database = serviceScope.ServiceProvider.GetRequiredService<DbService>();
                    var config = await database.GetOrCreateLevelConfigAsync(x.GuildId);
                    _cache.AdjustExpMultiplier(ExpSource.Text, x.GuildId, config.TextExpMultiplier);
                    _cache.AdjustExpMultiplier(ExpSource.Voice, x.GuildId, config.VoiceExpMultiplier);
                    _cache.AdjustExpMultiplier(ExpSource.Other, x.GuildId, 1);
                    _cache.TryRemoveExpEvent(x.GuildId);
                }, null, x.Time - DateTime.UtcNow, Timeout.InfiniteTimeSpan);
                _cache.TryAddExpEvent(x.GuildId, timer);
                _cache.AdjustExpMultiplier(ExpSource.Text, x.GuildId, x.Multiplier);
                _cache.AdjustExpMultiplier(ExpSource.Voice, x.GuildId, x.Multiplier);
                _cache.AdjustExpMultiplier(ExpSource.Other, x.GuildId, x.Multiplier);
            }
        }

        protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            if (e.Member.IsBot) return;
            var user = e.Member;
            var after = e.NewVoiceState;
            var before = e.OldVoiceState;
            try
            {
                if (before != null && after != null) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLevelConfigAsync(e.GuildId);
                if (!cfg.VoiceExpEnabled) return;
                var userData = await db.GetOrCreateUserData(user);
                if (before == null && after != null)
                {
                    userData.VoiceExpTime = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                    return;
                }
                
                if (before != null)
                {
                    var guild = _bot.GetGuild(e.GuildId);
                    var channel = guild.GetChannel(before.ChannelId.Value);
                    var time = DateTime.UtcNow - userData.VoiceExpTime;
                    var exp = GetExp((IVoiceChannel) channel, time);
                    userData.StatVoiceTime += time;
                    await AddExpAsync(user, userData, exp, Convert.ToInt32(exp / 2), db, ExpSource.Voice);
                    await _achievement.TotalTime(userData, db);
                }
            }
            catch (Exception z)
            {
                _logger.Log(LogLevel.Error, z,
                    $"Error in {e.GuildId} for Voice - {z.Message}");
            }
        }

        protected override async ValueTask OnMessageReceived(MessageReceivedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (e.Member.IsBot) return;
            
            var cdCheck = _cache.TryGetCooldown(e.GuildId.Value, e.Member.Id, CooldownType.ServerMessage);
            if (!cdCheck)
            {
                _cache.AddCooldown(e.GuildId.Value, e.Member.Id, CooldownType.ServerMessage);
                _ = ServerExperienceAsync(e);
                _ = _provider.GetRequiredService<DropService>().MessageReceived(e);
            }

            await GlobalExperienceAsync(e);
        }

        protected override async ValueTask OnRoleDeleted(RoleDeletedEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var levelRole =
                await db.LevelRewards.FirstOrDefaultAsync(x => x.GuildId == e.GuildId && x.Role == e.RoleId);
            if (levelRole == null) return;
            db.LevelRewards.Remove(levelRole);
            await db.SaveChangesAsync();
        }

        private async Task ServerExperienceAsync(MessageReceivedEventArgs e)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var userData = await db.GetOrCreateUserData(e.Member);
                userData.LastMessage = DateTime.UtcNow;
                userData.FirstMessage ??= DateTime.UtcNow;
                await AddExpAsync(e.Member, userData, GetExp(e.Channel), _random.Next(0, 3), db, ExpSource.Text);
                await GiveawayAsync(db, e.Member);
            }
            catch (Exception z)
            {
                _logger.Log(LogLevel.Error, z,
                    $"Error in {e.GuildId.Value} for Server Exp - {z.Message}");
            }
        }
        
        public async Task GlobalExperienceAsync(MessageReceivedEventArgs e)
        {
            if (e.Member == null) return;
            if (_cache.TryGetGlobalCooldown(e.Member.Id)) return;
            _cache.AddGlobalCooldown(e.Member.Id);
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateGlobalUserDataAsync(e.Member);
            await AddExpAsync(userData, GetExp(e.Channel), _random.Next(1, 3), db);
        }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
        
        private static async Task GiveawayAsync(DbService db, IMember user)
        {
            var giveaways = await db.Giveaways
                .Where(x => x.GuildId == user.GuildId && x.Type == GiveawayType.Activity && x.Active)
                .ToListAsync();
            if (giveaways.Count == 0) return;
            foreach (var x in giveaways.Where(x => x.Active).Where(x => !x.CloseAtOffset.HasValue || x.CloseAtOffset.Value < DateTimeOffset.UtcNow))
            {
                if (!x.Stack)
                {
                    var check = await db.GiveawayParticipants.FirstOrDefaultAsync(e =>
                        e.UserId == user.Id && e.GiveawayId == x.Id);
                    if(check != null) continue;
                }

                await db.GiveawayParticipants.AddAsync(new GiveawayParticipant
                {
                    Id = Guid.NewGuid(),
                    GuildId = user.GuildId,
                    UserId = user.Id,
                    Entry = DateTimeOffset.UtcNow,
                    GiveawayId = x.Id,
                    Giveaway = x
                });
            }

            await db.SaveChangesAsync();
        }
        
        private int GetExp(INestableChannel channel)
        {
            var xp = _random.Next(10, 20);
            if (IsReducedExp(channel.Id, channel.CategoryId)) xp = Convert.ToInt32(xp / 10);
            return xp;
        }
        
        private int GetExp(INestableChannel channel, TimeSpan period)
        {
            var xp = Convert.ToInt32(period.TotalMinutes * 2);
            if (IsReducedExp(channel.Id, channel.CategoryId)) xp = Convert.ToInt32(xp / 10);
            return xp;
        }

        private bool IsReducedExp(Snowflake channelId, Snowflake? categoryId)
        {
            if (categoryId.HasValue && _cache.IsExpReduced(categoryId.Value)) return true;
            return _cache.IsExpReduced(channelId);
        }
    }
}