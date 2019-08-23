using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Administration.Warning;
using Hanekawa.Bot.Services.Logging;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Extensions;
using Hanekawa.Shared.Interfaces;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Administration.Mute
{
    public partial class MuteService : INService
    {
        private readonly DiscordSocketClient _client;

        private readonly OverwritePermissions _denyOverwrite =
            new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny,
                attachFiles: PermValue.Deny);

        private readonly InternalLogService _log;
        private readonly LogService _logService;
        private readonly WarnService _warn;

        public MuteService(DiscordSocketClient client, LogService logService, InternalLogService log, WarnService warn)
        {
            _client = client;
            _logService = logService;
            _log = log;
            _warn = warn;

            using (var db = new DbService())
            {
                foreach (var x in db.MuteTimers)
                {
                    TimeSpan after;
                    if (x.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow) after = TimeSpan.FromMinutes(2);
                    else after = x.Time - DateTime.UtcNow;
                    StartUnMuteTimer(x.GuildId, x.UserId, after);
                }
            }
        }
        public async Task<bool> Mute(SocketGuildUser user, DbService db)
        {
            var role = await GetMuteRoleAsync(user.Guild, db);
            var check = await user.TryAddRoleAsync(role as SocketRole);
            if (!check) return false;
            _ = ApplyPermissions(user.Guild, role);
            await user.TryMute();
            _log.LogAction(LogLevel.Information, $"(Mute service) Muted {user.Id} in {user.Guild.Id}");
            return true;
        }

        public async Task<bool> UnMuteUser(SocketGuildUser user, DbService db)
        {
            await StopUnMuteTimerAsync(user.Guild.Id, user.Id, db);
            await user.TryUnMute();
            _log.LogAction(LogLevel.Information, $"(Mute service) Unmuted {user.Id} in {user.Guild.Id}");
            return await user.TryRemoveRoleAsync(await GetMuteRoleAsync(user.Guild, db) as SocketRole);
        }

        public async Task<bool> TimedMute(SocketGuildUser user, SocketGuildUser staff, TimeSpan after, DbService db,
            string reason)
        {
            await Mute(user, db).ConfigureAwait(false);
            var unMuteAt = DateTime.UtcNow + after;
            await db.MuteTimers.AddAsync(new MuteTimer
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Time = unMuteAt
            });
            await db.SaveChangesAsync();
            StartUnMuteTimer(user.Guild.Id, user.Id, after);
            await _logService.Mute(user, staff, reason, after, db);

            _log.LogAction(LogLevel.Information, $"(Mute service) {staff.Id} muted {user.Id} in {user.Guild.Id} for {after.Humanize()}");
            return true;
        }

        private async Task<IRole> GetMuteRoleAsync(SocketGuild guild, DbService db)
        {
            var cfg = await db.GetOrCreateAdminConfigAsync(guild);
            IRole role;
            if (!cfg.MuteRole.HasValue)
            {
                role = await CreateRole(guild);
                cfg.MuteRole = role.Id;
                await db.SaveChangesAsync();
            }
            else
            {
                role = guild.GetRole(cfg.MuteRole.Value);
            }

            return role;
        }

        private async Task<IRole> CreateRole(SocketGuild guild)
        {
            var role = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "mute");
            if (role == null) return await guild.CreateRoleAsync("Mute", GuildPermissions.None);
            return role;
        }

        private async Task ApplyPermissions(SocketGuild guild, IRole role)
        {
            foreach (var x in guild.TextChannels)
            {
                if (x.PermissionOverwrites.Select(z => z.Permissions).Contains(_denyOverwrite)) continue;
                try
                {
                    await x.TryApplyPermissionOverwriteAsync(role, _denyOverwrite)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Mute service) Couldn't apply permission overwrite in {x.Guild.Id} in channel {x.Id}");
                }

                await Task.Delay(200).ConfigureAwait(false);
            }
        }
    }
}