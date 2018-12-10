using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Events;
using Hanekawa.Services.AutoModerator;

namespace Hanekawa.Services.Administration
{
    public class MuteService : IHanaService
    {
        private const string DefaultMuteRole = "Mute";

        private static readonly OverwritePermissions DenyOverwrite =
            new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny,
                attachFiles: PermValue.Deny);

        private readonly DiscordSocketClient _client;
        private readonly ModerationService _moderationService;

        public MuteService(DiscordSocketClient client, ModerationService moderationService)
        {
            _client = client;
            _moderationService = moderationService;

            _moderationService.AutoModPermMute += AutoModPermMute;
            _moderationService.AutoModTimedMute += AutoModTimedMute;

            using (var db = new DbService())
            {
                foreach (var x in db.MuteTimers)
                {
                    TimeSpan after;
                    if (x.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow) after = TimeSpan.FromMinutes(2);
                    else after = x.Time - DateTime.UtcNow;
                    StartUnmuteTimer(x.GuildId, x.UserId, after);
                }
            }
            Console.WriteLine("Mute service loaded");
        }

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> UnmuteTimers { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();

        public event AsyncEvent<SocketGuildUser, SocketGuildUser> UserMuted;
        public event AsyncEvent<SocketGuildUser, SocketGuildUser, TimeSpan> UserTimedMuted;
        public event AsyncEvent<SocketGuildUser> UserUnmuted;

        // EVENTS
        private Task AutoModTimedMute(IGuildUser user, TimeSpan after)
        {
            var _ = Task.Run(async () => { await TimedMute(user, after); });
            return Task.CompletedTask;
        }

        private Task AutoModPermMute(IGuildUser arg1)
        {
            var _ = Task.Run(async () => { await Mute(arg1); });
            return Task.CompletedTask;
        }

        // MUTE AREA
        public async Task Mute(IGuildUser user)
        {
            using (var db = new DbService())
            {
                await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
                var muteRole = await GetMuteRole(user.Guild);
                if (!user.RoleIds.Contains(muteRole.Id)) await user.AddRoleAsync(muteRole).ConfigureAwait(false);
                await StopUnmuteTimerAsync(db, user.GuildId, user.Id);
            }
        }

        public async Task Mute(IGuildUser user, IGuildUser staff)
        {
            using (var db = new DbService())
            {
                await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
                var muteRole = await GetMuteRole(user.Guild);
                if (!user.RoleIds.Contains(muteRole.Id)) await user.AddRoleAsync(muteRole).ConfigureAwait(false);
                var stopTimer = StopUnmuteTimerAsync(db, user.GuildId, user.Id);
                var unmute = UserMuted?.Invoke(user as SocketGuildUser, staff as SocketGuildUser);
                await Task.WhenAll(stopTimer, unmute);
            }
        }

        // TIMED MUTE AREA
        public async Task TimedMute(IGuildUser user, IGuildUser staff, TimeSpan after)
        {
            await Mute(user).ConfigureAwait(false);
            using (var db = new DbService())
            {
                var unMuteAt = DateTime.UtcNow + after;
                var userCheck = await db.MuteTimers.FindAsync(user.Id, user.GuildId);
                if (userCheck == null)
                {
                    var data = new MuteTimer
                    {
                        GuildId = user.GuildId,
                        UserId = user.Id,
                        Time = unMuteAt
                    };
                    await db.MuteTimers.AddAsync(data);
                    await db.SaveChangesAsync();
                }
                else
                {
                    userCheck.Time = unMuteAt;
                    await db.SaveChangesAsync();
                }

                StartUnmuteTimer(user.GuildId, user.Id, after);
                await UserTimedMuted(user as SocketGuildUser, staff as SocketGuildUser, after);
            }
        }

        public async Task TimedMute(IGuildUser user, TimeSpan after)
        {
            await Mute(user).ConfigureAwait(false);
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

        private void StartUnmuteTimer(ulong guildId, ulong userId, TimeSpan after)
        {
            var userUnmuteTimers = UnmuteTimers.GetOrAdd(guildId, new ConcurrentDictionary<ulong, Timer>());

            var toAdd = new Timer(async _ =>
            {
                using (var db = new DbService())
                {
                    try
                    {
                        var guild = _client.GetGuild(guildId);
                        var user = guild.GetUser(userId);
                        await UnmuteUser(db, user);
                    }
                    catch
                    {
                        await RemoveTimerFromDbAsync(db, guildId, userId);
                    }
                }
            }, null, after, Timeout.InfiniteTimeSpan);

            userUnmuteTimers.AddOrUpdate(userId, key => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }

        private async Task StopUnmuteTimerAsync(DbService db, ulong guildId, ulong userId)
        {
            await RemoveTimerFromDbAsync(db, guildId, userId);
            if (!UnmuteTimers.TryGetValue(guildId, out var userUnmuteTimers)) return;
            if (!userUnmuteTimers.TryRemove(userId, out var removed)) return;
            removed.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private async Task RemoveTimerFromDbAsync(DbService db, ulong guildId, ulong userId)
        {
            try
            {
                var data = db.MuteTimers.FirstOrDefault(x => x.GuildId == guildId && x.UserId == userId);
                if (data == null) return;
                db.MuteTimers.Remove(data);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Unmute AREA
        public async Task UnmuteUser(DbService db, IGuildUser user)
        {
                await StopUnmuteTimerAsync(db, user.GuildId, user.Id);
                try
                {
                    await user.ModifyAsync(x => x.Mute = false).ConfigureAwait(false);
                }
                catch
                {
                    /*IGNORE*/
                }

                try
                {
                    await user.RemoveRoleAsync(await GetMuteRole(user.Guild)).ConfigureAwait(false);
                }
                catch
                {
                    /*IGNORE*/
                }

                await UserUnmuted(user as SocketGuildUser);
        }

        // GET ROLE AREA
        private static async Task<IRole> GetMuteRole(IGuild guild)
        {
            IRole muteRole;
            var defaultCheck = guild.Roles.FirstOrDefault(x => x.Name == DefaultMuteRole);
            if (defaultCheck == null)
            {
                var role = await guild.CreateRoleAsync(DefaultMuteRole, GuildPermissions.None)
                    .ConfigureAwait(false);
                muteRole = role;
            }
            else
            {
                muteRole = defaultCheck;
            }

            foreach (var toOverwrite in await guild.GetTextChannelsAsync())
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

            return muteRole;
        }
    }
}