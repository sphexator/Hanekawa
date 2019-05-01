using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Administration.Mute
{
    public partial class MuteService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> _unMuteTimers 
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();

        public async Task UnMuteUser(SocketGuildUser user, DbService db)
        {
            await StopUnMuteTimerAsync(user.Guild.Id, user.Id, db);
            try
            {
                await user.ModifyAsync(x => x.Mute = false).ConfigureAwait(false);
            }
            catch { /* Ignore */ }

            await user.TryRemoveRoleAsync(await GetMuteRoleAsync(user.Guild, db) as SocketRole);
        }

        private void StartUnMuteTimer(ulong guildId, ulong userId, TimeSpan duration)
        {
            var unMuteTimers = _unMuteTimers.GetOrAdd(guildId, new ConcurrentDictionary<ulong, Timer>());
            var toAdd = new Timer(async _ =>
            {
                using (var db = new DbService())
                {
                    try
                    {
                        var guild = _client.GetGuild(guildId);
                        var user = guild.GetUser(userId);
                        await UnMuteUser(user, db);
                    }
                    catch
                    {
                        await RemoveTimerFromDbAsync(guildId, userId, db);
                    }
                }
            }, null, duration, Timeout.InfiniteTimeSpan);

            unMuteTimers.AddOrUpdate(userId, key => toAdd, (key, old) =>
            {
                old.Dispose();
                return toAdd;
            });
        }

        private async Task StopUnMuteTimerAsync(ulong guildId, ulong userId, DbService db)
        {
            await RemoveTimerFromDbAsync(guildId, userId, db);
            if (!_unMuteTimers.TryGetValue(guildId, out var unMuteTimers)) return;
            if (!unMuteTimers.TryRemove(userId, out var removed)) return;
            removed.Dispose();
        }

        private async Task RemoveTimerFromDbAsync(ulong guildId, ulong userId, DbService db)
        {
            var data = await db.MuteTimers.FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId);
            if (data == null) return;
            db.MuteTimers.Remove(data);
            await db.SaveChangesAsync();
        }
    }
}