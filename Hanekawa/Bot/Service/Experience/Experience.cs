using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Giveaway;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Quartz;

namespace Hanekawa.Bot.Service
{
    public partial class Experience : INService, IJob
    {
        private readonly CacheService _cache;
        private readonly Logger _logger;
        private readonly Hanekawa _bot;
        private readonly IServiceProvider _provider;
        private readonly Random _random;

        public Experience(CacheService cache, Hanekawa bot, IServiceProvider provider, Random random)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _cache = cache;
            _bot = bot;
            _provider = provider;
            _random = random;
        }
        
        public async Task VoiceExperienceAsync(VoiceStateUpdatedEventArgs e)
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
                var cfg = await db.GetOrCreateLevelConfigAsync(e.GuildId.RawValue);
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
                    if(!guild.Channels.TryGetValue(before.ChannelId.Value, out var channel)) return;
                    var exp = GetExp((IVoiceChannel) channel, DateTime.UtcNow - userData.VoiceExpTime);
                    await AddExpAsync(user, userData, exp, Convert.ToInt32(exp / 2), db, ExpSource.Voice);
                }
            }
            catch (Exception z)
            {
                _logger.Log(LogLevel.Error, z,
                    $"(Exp Service) Error in {e.GuildId.RawValue} for Voice - {z.Message}");
            }
        }

        public async Task ServerExperienceAsync(MessageReceivedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (e.Member.IsBot) return;
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
                    $"(Exp Service) Error in {e.GuildId.Value.RawValue} for Server Exp - {z.Message}");
            }
        }

        public async Task GlobalExperienceAsync(MessageReceivedEventArgs e)
        {
            if (e.Member == null) return;
            if (_cache.GlobalCooldown.TryGetValue(e.Member.Id, out _)) return;
            _cache.GlobalCooldown.Set(e.Member.Id, 0, TimeSpan.FromMinutes(1));
            
        }

        public Task Execute(IJobExecutionContext context)
        {
            throw new System.NotImplementedException();
        }
        
        private async Task GiveawayAsync(DbService db, IMember user)
        {
            var giveaways = await db.Giveaways
                .Where(x => x.GuildId == user.GuildId.RawValue && x.Type == GiveawayType.Activity && x.Active)
                .ToListAsync();
            if (giveaways.Count == 0) return;
            foreach (var x in giveaways.Where(x => x.Active).Where(x => !x.CloseAtOffset.HasValue || x.CloseAtOffset.Value < DateTimeOffset.UtcNow))
            {
                if (!x.Stack)
                {
                    var x1 = x;
                    var check = await db.GiveawayParticipants.FirstOrDefaultAsync(e =>
                        e.UserId == user.Id.RawValue && e.GiveawayId == x1.Id);
                    if(check != null) continue;
                }

                await db.GiveawayParticipants.AddAsync(new GiveawayParticipant
                {
                    Id = Guid.NewGuid(),
                    GuildId = user.GuildId.RawValue,
                    UserId = user.Id.RawValue,
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
            if (IsReducedExp(channel.GuildId, channel.Id, channel.CategoryId)) xp = Convert.ToInt32(xp / 10);
            return xp;
        }
        
        private int GetExp(INestableChannel channel, TimeSpan period)
        {
            var xp = Convert.ToInt32(period.TotalMinutes * 2);
            if (IsReducedExp(channel.GuildId, channel.Id, channel.CategoryId)) xp = Convert.ToInt32(xp / 10);
            return xp;
        }

        private bool IsReducedExp(Snowflake guildId, Snowflake channelId, Snowflake? categoryId)
        {
            var cache = _cache.ExperienceReduction.GetOrAdd(guildId, new HashSet<Snowflake>());
            if (categoryId.HasValue && cache.TryGetValue(categoryId.Value, out _)) return true;
            return cache.TryGetValue(channelId, out _);
        }
    }
}