using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Administration.Mute
{
    public partial class MuteService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> _unMuteTimers 
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();

        public async Task UnMuteUser(SocketGuildUser user)
        {
            await StopUnMuteTimerAsync(user.Guild.Id, user.Id);
            try
            {
                await user.ModifyAsync(x => x.Mute = false).ConfigureAwait(false);
            }
            catch { /* Ignore */ }

            await user.TryRemoveRoleAsync(await GetMuteRoleAsync(user.Guild) as SocketRole);
        }

        private void StartUnMuteTimer(ulong guildId, ulong userId, TimeSpan duration)
        {
            var unMuteTimers = _unMuteTimers.GetOrAdd(guildId, new ConcurrentDictionary<ulong, Timer>());
            var toAdd = new Timer(async _ =>
            {
                try
                {
                    var guild = _client.GetGuild(guildId);
                    var user = guild.GetUser(userId);
                    await UnMuteUser(user);
                }
                catch
                {
                    await RemoveTimerFromDbAsync(guildId, userId);
                }
            }, null, duration, Timeout.InfiniteTimeSpan);

            unMuteTimers.AddOrUpdate(userId, key => toAdd, (key, old) =>
            {
                old.Dispose();
                return toAdd;
            });
        }

        private async Task StopUnMuteTimerAsync(ulong guildId, ulong userId)
        {
            await RemoveTimerFromDbAsync(guildId, userId);
            if (!_unMuteTimers.TryGetValue(guildId, out var unMuteTimers)) return;
            if (!unMuteTimers.TryRemove(userId, out var removed)) return;
            removed.Dispose();
        }

        private async Task RemoveTimerFromDbAsync(ulong guildId, ulong userId)
        {
            var data = await _db.MuteTimers.FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId);
            if (data == null) return;
            _db.MuteTimers.Remove(data);
            await _db.SaveChangesAsync();
        }
    }
}