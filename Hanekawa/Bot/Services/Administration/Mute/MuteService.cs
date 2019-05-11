using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Administration.Warning;
using Hanekawa.Bot.Services.Logging;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Extensions;

namespace Hanekawa.Bot.Services.Administration.Mute
{
    public partial class MuteService : INService
    {
        private readonly DiscordSocketClient _client;
        private readonly LogService _logService;
        private readonly InternalLogService _log;
        private readonly WarnService _warn;

        private readonly OverwritePermissions _denyOverwrite =
            new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny,
                attachFiles: PermValue.Deny);

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

        public async Task<bool> TimedMute(SocketGuildUser user, SocketGuildUser staff, TimeSpan after, DbService db, string reason)
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
            return true;
        }

        public async Task<bool> Mute(SocketGuildUser user, DbService db)
        {
            var role = await GetMuteRoleAsync(user.Guild, db);
            var check = await user.TryAddRoleAsync(role as SocketRole);
            if (!check) return false;
            _ = ApplyPermissions(user.Guild, role);
            await user.TryMute();
            return true;
        }

        public async Task<bool> UnMuteUser(SocketGuildUser user, DbService db)
        {
            await StopUnMuteTimerAsync(user.Guild.Id, user.Id, db);
            await user.TryUnMute();

            return await user.TryRemoveRoleAsync(await GetMuteRoleAsync(user.Guild, db) as SocketRole);
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
            else role = guild.GetRole(cfg.MuteRole.Value);

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
                await x.AddPermissionOverwriteAsync(role, _denyOverwrite)
                    .ConfigureAwait(false);

                await Task.Delay(200).ConfigureAwait(false);
            }
        }
    }
}
