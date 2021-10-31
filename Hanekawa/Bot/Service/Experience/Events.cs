using System;
using System.Threading;
using System.Threading.Tasks;
using Hanekawa.Bot.Commands;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Service.Experience
{
    public partial class ExpService
    {
        /// <summary>
        /// Start a experience event, setting a multiplier for all experience sources.
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="context">Command Context</param>
        /// <param name="multiplier">Experience Multiplier</param>
        /// <param name="duration">Timespan duration</param>
        public async Task StartEventAsync(DbService db, HanekawaCommandContext context, double multiplier,
            TimeSpan duration)
        {
            // TODO: Recreate Exp Event
            var check = await db.LevelExpEvents.FindAsync(context.GuildId);
            var timer = new Timer(async _ =>
            {
                using var scope = _provider.CreateScope();
                await using var database = scope.ServiceProvider.GetRequiredService<DbService>();
                var config = await database.GetOrCreateEntityAsync<LevelConfig>(context.GuildId);
                _cache.AdjustExpMultiplier(ExpSource.Text, context.GuildId, config.TextExpMultiplier);
                _cache.AdjustExpMultiplier(ExpSource.Voice, context.GuildId, config.VoiceExpMultiplier);
                _cache.AdjustExpMultiplier(ExpSource.Other, context.GuildId, 1);
                _cache.TryRemoveExpEvent(context.GuildId);
            }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            try
            {
                if (check != null)
                {
                    check.Start = DateTimeOffset.UtcNow;
                    check.End = DateTimeOffset.UtcNow.Add(duration);
                    check.Multiplier = multiplier;
                }
                else
                {
                    await db.LevelExpEvents.AddAsync(new()
                    {
                        GuildId = context.Guild.Id,
                        Multiplier = multiplier,
                        Start = DateTimeOffset.UtcNow,
                        End = DateTimeOffset.UtcNow.Add(duration)
                    });
                }
                await db.SaveChangesAsync();
                timer.Change(duration, Timeout.InfiniteTimeSpan);
                _cache.TryAddExpEvent(context.GuildId, timer);
                _cache.AdjustExpMultiplier(ExpSource.Text, context.GuildId, multiplier);
                _cache.AdjustExpMultiplier(ExpSource.Voice, context.GuildId, multiplier);
                _cache.AdjustExpMultiplier(ExpSource.Other, context.GuildId, multiplier);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error in exp event creation");
                await timer.DisposeAsync();
                var config = await db.GetOrCreateEntityAsync<LevelConfig>(context.GuildId);
                _cache.AdjustExpMultiplier(ExpSource.Text, context.GuildId, config.TextExpMultiplier);
                _cache.AdjustExpMultiplier(ExpSource.Voice, context.GuildId, config.VoiceExpMultiplier);
                _cache.AdjustExpMultiplier(ExpSource.Other, context.GuildId, 1);
            }
        }
    }
}