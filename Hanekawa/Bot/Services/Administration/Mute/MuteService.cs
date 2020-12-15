using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Hanekawa.Bot.Services.Logging;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Administration.Mute
{
    public partial class MuteService : INService, IRequired
    {
        private readonly Hanekawa _client;

        private readonly OverwritePermissions _denyOverwrite 
            = new OverwritePermissions(ChannelPermissions.None, new ChannelPermissions(34880));
        private readonly InternalLogService _log;
        private readonly ColourService _colour;
        private readonly LogService _logService;
        private readonly IServiceProvider _provider;

        public MuteService(Hanekawa client, LogService logService, InternalLogService log, IServiceProvider provider, ColourService colour)
        {
            _client = client;
            _logService = logService;
            _log = log;
            _provider = provider;
            _colour = colour;

            _client.MemberJoined += MuteCheck;

            using var scope = _provider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            foreach (var x in db.MuteTimers)
            {
                try
                {
                    var after = x.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow
                        ? TimeSpan.FromMinutes(2)
                        : x.Time - DateTime.UtcNow;
                    StartUnMuteTimer(x.GuildId, x.UserId, after);
                }
                catch (Exception e)
                {
                    db.Remove(x);
                    _log.LogAction(LogLevel.Error, e, $"(Mute Service) Couldn't create unmute timer in {x.GuildId} for {x.UserId}");
                }
            }
        }

        private Task MuteCheck(MemberJoinedEventArgs e)
        {
            if (e.Member.IsBot) return Task.CompletedTask;
            _ = Task.Run(async () =>
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var check = await db.MuteTimers.FindAsync(e.Member.Id.RawValue, e.Member.Guild.Id.RawValue);
                if (check == null) return;
                if(!await Mute(e.Member, db)) return;
                var muteTimers = _unMuteTimers.GetOrAdd(e.Member.Guild.Id.RawValue, new ConcurrentDictionary<ulong, Timer>());
                if (muteTimers.TryGetValue(e.Member.Id, out _)) return;
                var after = check.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow
                    ? TimeSpan.FromMinutes(2)
                    : check.Time - DateTime.UtcNow;
                StartUnMuteTimer(e.Member.Guild.Id.RawValue, e.Member.Id.RawValue, after);
            });
            return Task.CompletedTask;
        }

        public async Task<bool> Mute(CachedMember user, DbService db)
        {
            var role = await GetMuteRoleAsync(user.Guild, db);
            var check = await user.TryAddRoleAsync(role as CachedRole);
            if (!check) return false;
            _ = ApplyPermissions(user.Guild, role);
            await user.TryMute();
            _log.LogAction(LogLevel.Information, $"(Mute service) Muted {user.Id.RawValue} in {user.Guild.Id.RawValue}");
            return true;
        }

        public async Task<bool> UnMuteUser(CachedMember user, DbService db)
        {
            await StopUnMuteTimerAsync(user.Guild.Id.RawValue, user.Id.RawValue, db);
            await user.TryUnMute();
            _log.LogAction(LogLevel.Information, $"(Mute service) Unmuted {user.Id.RawValue} in {user.Guild.Id.RawValue}");
            return await user.TryRemoveRoleAsync(await GetMuteRoleAsync(user.Guild, db) as CachedRole);
        }

        public async Task<bool> TimedMute(CachedMember user, CachedMember staff, TimeSpan after, DbService db,
            string reason)
        {
            await Mute(user, db).ConfigureAwait(false);
            var unMuteAt = DateTime.UtcNow + after;
            var muteCheck = await db.MuteTimers.FindAsync(user.Id.RawValue, user.Guild.Id.RawValue);
            if (muteCheck == null)
            {
                await db.MuteTimers.AddAsync(new MuteTimer
                {
                    GuildId = user.Guild.Id.RawValue,
                    UserId = user.Id.RawValue,
                    Time = unMuteAt
                });
            }
            else
            {
                muteCheck.Time = unMuteAt;
            }
            await db.SaveChangesAsync();
            StartUnMuteTimer(user.Guild.Id.RawValue, user.Id.RawValue, after);
            await _logService.Mute(user, staff, reason, after, db);
            await NotifyUser(user, after);
            _log.LogAction(LogLevel.Information, $"(Mute service) {staff.Id.RawValue} muted {user.Id.RawValue} in {user.Guild.Id.RawValue} for {after.Humanize(2)}");
            return true;
        }

        public async Task<TimeSpan> GetMuteTime(CachedMember user, DbService db)
        {
            var warns = await db.Warns.Where(x =>
                x.GuildId == user.Guild.Id.RawValue && 
                x.UserId == user.Id.RawValue &&
                x.Type == WarnReason.Muted &&
                x.Time >= DateTime.UtcNow.AddDays(-30)).ToListAsync();
            return warns == null || warns.Count == 0 
                ? TimeSpan.FromHours(1) 
                : TimeSpan.FromHours(warns.Count + 2);
        }

        private async Task NotifyUser(CachedMember user, TimeSpan duration)
        {
            try
            {
                await user.SendMessageAsync(null, false,
                    new LocalEmbedBuilder().Create($"You've been muted in {user.Guild.Name} for {duration.Humanize()}", _colour.Get(user.Guild.Id.RawValue)).Build());
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, "Couldn't DM user");
            }
        }

        private async Task<IRole> GetMuteRoleAsync(CachedGuild guild, DbService db)
        {
            var cfg = await db.GetOrCreateAdminConfigAsync(guild);
            var role = !cfg.MuteRole.HasValue
                ? await CreateRole(guild, cfg, db)
                : guild.GetRole(cfg.MuteRole.Value) ?? await CreateRole(guild, cfg, db);
            return role;
        }

        private static async Task<IRole> CreateRole(CachedGuild guild, AdminConfig cfg, DbService db)
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
            cfg.MuteRole = role.Id.RawValue;
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
                    _log.LogAction(LogLevel.Error, e, $"(Mute service) Couldn't apply permission overwrite in {x.Value.Guild.Id.RawValue} in channel {x.Key}");
                }

                await Task.Delay(200).ConfigureAwait(false);
            }
        }
    }
}