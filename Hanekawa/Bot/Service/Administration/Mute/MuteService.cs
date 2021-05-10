﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Bot.Service.Administration.Warning;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Logs;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using LogLevel = NLog.LogLevel;

namespace Hanekawa.Bot.Service.Administration.Mute
{
    public class MuteService : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly Logger _logger;
        private readonly LogService _logService;
        private readonly CacheService _cache;
        private readonly WarnService _warn;
        private readonly Hanekawa _bot;
        private readonly OverwritePermissions _deny = new(ChannelPermissions.None, new ChannelPermissions(34880));

        public MuteService(IServiceProvider provider, LogService logService, CacheService cache, Hanekawa bot, 
            WarnService warn, ILogger<MuteService> logger) : base(logger, bot)
        {
            _provider = provider;
            _logService = logService;
            _cache = cache;
            _bot = bot;
            _warn = warn;
            _logger = LogManager.GetCurrentClassLogger();
            
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
                    _logger.Log(LogLevel.Error, e, $"(Mute Service) Couldn't create unmute timer in {x.GuildId} for {x.UserId}");
                }
            }
            db.SaveChanges();
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.MuteTimers.FindAsync(e.Member.Id.RawValue, e.GuildId.RawValue);
            if (check == null) return;
            if(!await Mute(e.Member, await GetMuteRoleAsync(e.GuildId, db))) return;
            var after = check.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow
                ? TimeSpan.FromMinutes(2)
                : check.Time - DateTime.UtcNow;
            StartUnMuteTimer(e.GuildId.RawValue, e.Member.Id.RawValue, after);
        }

        public async Task<bool> Mute(IMember user, IMember staff, string reason, DbService db, TimeSpan? duration = null)
        {
            var role = await GetMuteRoleAsync(user.GuildId, db);
            if(!await Mute(user, role)) return false;
            await _logService.MuteAsync(user, staff, reason, db, duration);
            await _warn.Warn(user, staff, reason, WarnReason.Muted, true, db, duration);
            if (!duration.HasValue) return true;
            var date = DateTime.UtcNow + duration.Value;
            var muteCheck = await db.MuteTimers.FindAsync(user.Id.RawValue, user.GuildId.RawValue);
            if (muteCheck != null) muteCheck.Time = date;
            else
            {
                await db.MuteTimers.AddAsync(new MuteTimer
                {
                    UserId = user.Id.RawValue,
                    GuildId = user.GuildId.RawValue,
                    Time = date
                });
            } 
            
            await db.SaveChangesAsync();
            StartUnMuteTimer(user.GuildId, user.Id, duration.Value);
            _logger.Log(LogLevel.Info, $"(Mute service) Muted {user.Id.RawValue} in {user.GuildId.RawValue}");
            return true;
        }

        private async Task<bool> Mute(IMember user, IRole role)
        {
            var check = await user.TryAddRoleAsync(role);
            var roles = user.GetRoles();
            var newRoles = roles.Keys.ToList();
            newRoles.Add(role.Id);
            try
            {
                await user.ModifyAsync(x =>
                {
                    x.RoleIds = newRoles;
                    x.Mute = true;
                });
            }
            catch
            {
                await user.TryAddRoleAsync(role);
            }
            if(check) _logger.Log(LogLevel.Info, $"(Mute service) Muted {user.Id.RawValue} in {user.GuildId.RawValue}");
            else _logger.Log(LogLevel.Warn, $"(Mute service) Failed to mute {user.Id.RawValue} in {user.GuildId.RawValue}");
            return check;
        }
        
        public async Task<bool> UnMuteAsync(IMember user, DbService db)
        {
            var role = await GetMuteRoleAsync(user.GuildId, db);
            var roles = user.GetRoles();
            var newRoles = roles.Keys.ToList();
            newRoles.Remove(role.Id);
            await StopUnMuteTimerAsync(user.GuildId, user.Id, db);
            try
            {
                await user.ModifyAsync(x =>
                {
                    x.RoleIds = newRoles;
                    x.Mute = false;
                });
            }
            catch
            {
                var check = await user.TryRemoveRoleAsync(await GetMuteRoleAsync(user.GuildId, db));
                if (!check) return false;
            }
            _logger.Log(LogLevel.Info, $"(Mute service) Unmuted {user.Id.RawValue} in {user.GuildId.RawValue}");
            return true;
        }
        
        public async Task<TimeSpan> GetMuteTimeAsync(IMember user, DbService db)
        {
            var warns = await db.Warns.Where(x =>
                x.GuildId == user.GuildId && 
                x.UserId == user.Id.RawValue &&
                x.Type == WarnReason.Muted &&
                x.Time >= DateTime.UtcNow.AddDays(-30)).ToListAsync();
            return warns == null || warns.Count == 0 
                ? TimeSpan.FromHours(1) 
                : TimeSpan.FromHours(warns.Count + 2);
        }
        
        private async Task<IRole> GetMuteRoleAsync(Snowflake guildId, DbService db)
        {
            var cfg = await db.GetOrCreateAdminConfigAsync(guildId);
            if (cfg.MuteRole.HasValue && _bot.GetGuild(guildId).Roles.TryGetValue(cfg.MuteRole.Value, out var muteRole))
                return muteRole;
            muteRole = await CreateRoleAsync(guildId, cfg, db);
            return muteRole;
        }
        
        private async ValueTask<IRole> CreateRoleAsync(Snowflake guildId, AdminConfig cfg, DbService db)
        {
            var guild = _bot.GetGuild(guildId);
            var role = guild.Roles.FirstOrDefault(x => x.Value.Name.ToLower() == "mute" && guild.HierarchyCheck(guild.GetCurrentUser(), x.Value)).Value;
            
            if (role == null)
            {
                var cRole = await guild.CreateRoleAsync(x =>
                {
                    x.Name = "mute";
                    x.Permissions = Optional<GuildPermissions>.Empty;
                    x.IsHoisted = false;
                    x.IsMentionable = false;
                    x.Color = new Optional<Color?>(HanaBaseColor.Red());
                });
                role = cRole;
            }
            cfg.MuteRole = role.Id.RawValue;
            await db.SaveChangesAsync();
            return role;
        }
        
        private void StartUnMuteTimer(Snowflake guildId, Snowflake userId, TimeSpan duration)
        {
            try
            {
                var toAdd = new Timer(async _ =>
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    try
                    {
                        var guild = _bot.GetGuild(guildId);
                        if (guild == null)
                        {
                            await RemoveFromDatabaseAsync(guildId, userId, db);
                            return;
                        }
                        var user = await guild.FetchMemberAsync(userId);
                        if (user == null)
                        {
                            await RemoveFromDatabaseAsync(guildId, userId, db);
                            return;
                        }
                        await UnMuteAsync(user, db);
                    }
                    catch (Exception e)
                    {
                        await RemoveFromDatabaseAsync(guildId, userId, db);
                        _logger.Log(LogLevel.Error, e,
                            $"(Mute Service) Error for {userId} in {guildId} for UnMute - {e.Message}");
                    }
                }, null, duration, Timeout.InfiniteTimeSpan);
                
                _cache.AddOrUpdateMuteTimer(guildId, userId, toAdd);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, $"(Mute Service) Couldn't create unmute timer in {guildId} for {userId}");
            }
        }

        private async ValueTask StopUnMuteTimerAsync(Snowflake guildId, Snowflake userId, DbService db)
        {
            await RemoveFromDatabaseAsync(guildId, userId, db);
            _cache.RemoveMuteTimer(guildId, userId);
        }
        
        private static async ValueTask RemoveFromDatabaseAsync(Snowflake guildId, Snowflake userId, DbService db)
        {
            var data = await db.MuteTimers.FindAsync(guildId, userId);
            if (data == null) return;
            await RemoveFromDatabaseAsync(data, db);
        }
        
        private static async ValueTask RemoveFromDatabaseAsync(MuteTimer timer, DbService db)
        {
            db.MuteTimers.Remove(timer);
            await db.SaveChangesAsync();
        }

        private async Task ApplyPermissions(IGatewayGuild guild, IRole role)
        {
            foreach (var (_, ch) in guild.Channels)
            {
                await ApplyPermissions(ch, role);
            }
        }
        
        private async Task ApplyPermissions(IGuildChannel ch, IRole role)
        {
            if (ch is not CachedTextChannel channel) return;
            if (ch.Overwrites.Select(z => z.Permissions).Contains(_deny)) return;
            try
            {
                await channel.TryApplyPermissionOverwriteAsync(new LocalOverwrite(role, _deny))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, $"(Mute service) Couldn't apply permission overwrite in {ch.GuildId.RawValue} in channel {ch.Id.RawValue}");
            }

            await Task.Delay(200).ConfigureAwait(false);
        }
    }
}
