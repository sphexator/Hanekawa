using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;

namespace Jibril.Services.Administration
{
    public class MuteService
    {
        public enum MuteType
        {
            Voice,
            Chat,
            All
        }

        private const string DefaultMuteRole = "Mute";
        private readonly DiscordSocketClient _client;

        public event Action<IGuildUser, MuteType> UserMuted = delegate { };
        public event Action<IGuildUser, MuteType> UserUnmuted = delegate { };

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> UnmuteTimers { get; set; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();
        private ConcurrentDictionary<ulong, ulong> MuteRole { get; set; }
            = new ConcurrentDictionary<ulong, ulong>();

        private static readonly OverwritePermissions DenyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);

        public MuteService(DiscordSocketClient client)
        {
            _client = client;
            using (var db = new DbService())
            {
                foreach (var x in db.MuteTimers)
                {
                    TimeSpan after;
                    if (x.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow) after = TimeSpan.FromMinutes(2);
                    else after = x.Time - DateTime.UtcNow;
                    StartUnmuteTimer(x.GuildId, x.UserId, after);
                }

                foreach (var x in db.GuildConfigs)
                {
                    if (!x.MuteRole.HasValue) continue;
                    MuteRole.TryAdd(x.GuildId, x.MuteRole.Value);
                }
            }
        }

        public async Task MuteUser(IGuildUser user, MuteType type = MuteType.All)
        {
            await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            var muteRole = await GetMuteRole(user.Guild);
            if (!user.RoleIds.Contains(muteRole.Id)) await user.AddRoleAsync(muteRole).ConfigureAwait(false);
            StopUnmuteTimer(user.GuildId, user.Id);
            UserMuted(user, type);
        }

        public async Task TimedMute(IGuildUser user, TimeSpan after)
        {
            await MuteUser(user).ConfigureAwait(false);
            using (var db = new DbService())
            {
                var unMuteAt = DateTime.UtcNow + after;
                var data = new MuteTimer
                {
                    GuildId = user.GuildId,
                    UserId = user.Id,
                    Time = unMuteAt
                };
                await db.MuteTimers.AddAsync(data);
                await db.SaveChangesAsync();
            }
            StartUnmuteTimer(user.GuildId, user.Id, after);
        }

        public async Task UnmuteUser(IGuildUser user, MuteType type = MuteType.All)
        {
            StopUnmuteTimer(user.GuildId, user.Id);
            try{await user.ModifyAsync(x => x.Mute = false).ConfigureAwait(false);} catch {/*IGNORE*/}
            try{await user.RemoveRoleAsync(await GetMuteRole(user.Guild)).ConfigureAwait(false);} catch {/*IGNORE*/}
            
            UserUnmuted(user, type);
        }

        private async Task<IRole> GetMuteRole(IGuild guild)
        {
            var check = MuteRole.TryGetValue(guild.Id, out var roleId);
            IRole muteRole;
            if (!check)
            {
                muteRole = await guild.CreateRoleAsync(DefaultMuteRole, GuildPermissions.None).ConfigureAwait(false);
            }
            else muteRole = guild.Roles.FirstOrDefault(x => x.Id == roleId);

            foreach (var toOverwrite in (await guild.GetTextChannelsAsync()))
            {
                try
                {
                    if (toOverwrite.PermissionOverwrites.Select(x => x.Permissions).Contains(DenyOverwrite)) continue;
                    await toOverwrite.AddPermissionOverwriteAsync(muteRole, DenyOverwrite)
                        .ConfigureAwait(false);

                    await Task.Delay(200).ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }

            return muteRole;
        }

        private void StartUnmuteTimer(ulong guildId, ulong userId, TimeSpan after)
        {
            var userUnmuteTimers = UnmuteTimers.GetOrAdd(guildId, new ConcurrentDictionary<ulong, Timer>());

            var toAdd = new Timer(async _ =>
            {
                try
                {
                    var guild = _client.GetGuild(guildId);
                    var user = guild.GetUser(userId);
                    await UnmuteUser(user).ConfigureAwait(false);
                    UserUnmuted(user, MuteType.All);
                }
                catch
                {
                    RemoveTimerFromDb(guildId, userId);
                }
            }, null, after, Timeout.InfiniteTimeSpan);

            userUnmuteTimers.AddOrUpdate(userId, (key) => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }

        private void StopUnmuteTimer(ulong guildId, ulong userId)
        {
            if (!UnmuteTimers.TryGetValue(guildId, out var userUnmuteTimers)) return;

            if (userUnmuteTimers.TryRemove(userId, out var removed))
            {
                removed.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private static void RemoveTimerFromDb(ulong guildId, ulong userId)
        {
            using (var db = new DbService())
            {
                var data = db.MuteTimers.Find(guildId, userId);
                if (data == null) return;
                db.MuteTimers.Remove(data);
                db.SaveChanges();
            }
        }
    }
}
