using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Administration.Mute
{
    public partial class MuteService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> _unMuteTimers
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();

        private void StartUnMuteTimer(ulong guildId, ulong userId, TimeSpan duration)
        {
            try
            {
                var unMuteTimers = _unMuteTimers.GetOrAdd(guildId, new ConcurrentDictionary<ulong, Timer>());
                var toAdd = new Timer(async _ =>
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    try
                    {
                        var guild = _client.GetGuild(guildId);
                        if (guild == null)
                        {
                            await RemoveTimerFromDbAsync(guildId, userId, db);
                            return;
                        }
                        var user = await guild.GetOrFetchMemberAsync(userId);
                        if (user == null)
                        {
                            await RemoveTimerFromDbAsync(guildId, userId, db);
                            return;
                        }
                        await UnMuteUser(user as CachedMember, db);
                    }
                    catch (Exception e)
                    {
                        await RemoveTimerFromDbAsync(guildId, userId, db);
                        _log.Log(NLog.LogLevel.Error, e,
                            $"(Mute Service) Error for {userId} in {guildId} for UnMute - {e.Message}");
                    }
                }, null, duration, Timeout.InfiniteTimeSpan);

                unMuteTimers.AddOrUpdate(userId, key => toAdd, (key, old) =>
                {
                    old.Dispose();
                    return toAdd;
                });
            }
            catch (Exception e)
            {
                _log.Log(NLog.LogLevel.Error, e, $"(Mute Service) Couldn't create unmute timer in {guildId} for {userId}");
            }
        }

        private async Task StopUnMuteTimerAsync(ulong guildId, ulong userId, DbService db)
        {
            await RemoveTimerFromDbAsync(guildId, userId, db);
            if (!_unMuteTimers.TryGetValue(guildId, out var unMuteTimers)) return;
            if (!unMuteTimers.TryRemove(userId, out var removed)) return;
            await removed.DisposeAsync();
        }

        private static async Task RemoveTimerFromDbAsync(ulong guildId, ulong userId, DbService db)
        {
            var data = await db.MuteTimers.FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId);
            if (data == null) return;
            db.MuteTimers.Remove(data);
            await db.SaveChangesAsync();
        }
    }
}