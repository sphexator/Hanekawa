using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Shared.Command;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService
    {
        private Tuple<ulong, Timer> _expEventTimer;

        public async Task StartEventAsync(DbService db, HanekawaContext context, double multiplier, TimeSpan duration,
            bool announce = false)
        {
            var checkExisting = await db.LevelExpEvents.FindAsync(context.Guild.Id.RawValue);
            if (checkExisting != null && _expEventTimer != null)
            {
                checkExisting.Time = DateTime.UtcNow + duration;
                checkExisting.Multiplier = multiplier;
                if (_expEventTimer.Item1 == context.Guild.Id.RawValue) _expEventTimer.Item2.Dispose();
            }
            else
            {
                await db.LevelExpEvents.AddAsync(new LevelExpEvent
                {
                    GuildId = context.Guild.Id.RawValue,
                    Multiplier = multiplier,
                    Time = DateTime.UtcNow + duration,
                    ChannelId = null,
                    MessageId = null
                });
            }

            await db.SaveChangesAsync();
            _voiceExpMultiplier.AddOrUpdate(context.Guild.Id.RawValue, multiplier, (k, v) => multiplier);
            _textExpMultiplier.AddOrUpdate(context.Guild.Id.RawValue, multiplier, (key, v) => multiplier);
        }

        private async Task EventHandler(CancellationToken stopToken)
        {
            while (stopToken.IsCancellationRequested)
            {
                using (var db = new DbService())
                {
                    var nextEvent = await db.LevelExpEvents.OrderBy(x => x.Time).FirstOrDefaultAsync(stopToken);

                    if (_expEventTimer != null && nextEvent != null)
                    {
                        if (nextEvent.Time <= DateTime.UtcNow)
                        {
                            var cfg = await db.GetOrCreateLevelConfigAsync(nextEvent.GuildId);

                            _voiceExpMultiplier.AddOrUpdate(nextEvent.GuildId, cfg.VoiceExpMultiplier,
                                (k, v) => cfg.VoiceExpMultiplier);
                            _textExpMultiplier.AddOrUpdate(nextEvent.GuildId, cfg.TextExpMultiplier,
                                (k, v) => cfg.TextExpMultiplier);

                            db.LevelExpEvents.Remove(nextEvent);
                            await db.SaveChangesAsync(stopToken);
                        }
                        else
                        {
                            var timer = new Timer(async _ =>
                                {
                                    using var dbService = new DbService();
                                    var cfg = await dbService.GetOrCreateLevelConfigAsync(nextEvent.GuildId);

                                    _voiceExpMultiplier.AddOrUpdate(nextEvent.GuildId, cfg.VoiceExpMultiplier,
                                        (k, v) => cfg.VoiceExpMultiplier);
                                    _textExpMultiplier.AddOrUpdate(nextEvent.GuildId, cfg.TextExpMultiplier,
                                        (k, v) => cfg.TextExpMultiplier);

                                    dbService.LevelExpEvents.Remove(nextEvent);
                                    await dbService.SaveChangesAsync(stopToken);
                                }, null, nextEvent.Time - DateTime.UtcNow, Timeout.InfiniteTimeSpan);
                            _expEventTimer = new Tuple<ulong, Timer>(nextEvent.GuildId, timer);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stopToken);
            }
        }
    }
}