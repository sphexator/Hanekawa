using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Services.Administration.Warning;
using Hanekawa.Bot.Services.Logging;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Extensions;
using Hanekawa.Shared.Interfaces;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Administration.Mute
{
    public partial class MuteService : INService, IRequired
    {
        private readonly DiscordClient _client;

        private readonly OverwritePermissions _denyOverwrite 
            = new OverwritePermissions(ChannelPermissions.None, new ChannelPermissions(34880));
        private readonly InternalLogService _log;
        private readonly LogService _logService;
        private readonly WarnService _warn;

        public MuteService(DiscordClient client, LogService logService, InternalLogService log, WarnService warn)
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
        public async Task<bool> Mute(CachedMember user, DbService db)
        {
            var role = await GetMuteRoleAsync(user.Guild, db);
            var check = await user.TryAddRoleAsync(role as CachedRole);
            if (!check) return false;
            _ = ApplyPermissions(user.Guild, role);
            await user.TryMute();
            _log.LogAction(LogLevel.Information, $"(Mute service) Muted {user.Id} in {user.Guild.Id}");
            return true;
        }

        public async Task<bool> UnMuteUser(CachedMember user, DbService db)
        {
            await StopUnMuteTimerAsync(user.Guild.Id, user.Id, db);
            await user.TryUnMute();
            _log.LogAction(LogLevel.Information, $"(Mute service) Unmuted {user.Id} in {user.Guild.Id}");
            return await user.TryRemoveRoleAsync(await GetMuteRoleAsync(user.Guild, db) as CachedRole);
        }

        public async Task<bool> TimedMute(CachedMember user, CachedMember staff, TimeSpan after, DbService db,
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

            _log.LogAction(LogLevel.Information, $"(Mute service) {staff.Id} muted {user.Id} in {user.Guild.Id} for {after.Humanize(2)}");
            return true;
        }

        private async Task<IRole> GetMuteRoleAsync(CachedGuild guild, DbService db)
        {
            var cfg = await db.GetOrCreateAdminConfigAsync(guild);
            IRole role;
            if (!cfg.MuteRole.HasValue) role = await CreateRole(guild, cfg, db);
            else role = guild.GetRole(cfg.MuteRole.Value) ?? await CreateRole(guild, cfg, db);
            return role;
        }

        private async Task<IRole> CreateRole(CachedGuild guild, AdminConfig cfg, DbService db)
        {
            var role = guild.Roles.FirstOrDefault(x => x.Value.Name.ToLower() == "mute").Value as IRole;
            if (role == null)
            {
                var cRole = await guild.CreateRoleAsync(x =>
                {
                    x.Name = "mute";
                    x.Permissions = Optional<GuildPermissions>.Empty;
                });
                role = cRole;
            }
            cfg.MuteRole = role.Id;
            await db.SaveChangesAsync();
            return role;
        }

        private async Task ApplyPermissions(CachedGuild guild, IRole role)
        {
            for (var i = 0; i < guild.TextChannels.Count; i++)
            {
                var x = guild.TextChannels.ElementAt(i);
                if (x.Value.Overwrites.Select(z => z.Permissions).Contains(_denyOverwrite)) continue;
                try
                {
                    await x.Value.TryApplyPermissionOverwriteAsync(new LocalOverwrite(role, _denyOverwrite))
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Mute service) Couldn't apply permission overwrite in {x.Value.Guild.Id} in channel {x.Key}");
                }

                await Task.Delay(200).ConfigureAwait(false);
            }
        }
    }
}