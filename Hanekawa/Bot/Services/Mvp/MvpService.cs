using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Hanekawa.Bot.Services.Mvp
{
    public class MvpService : INService, IRequired, IJob
    {
        private readonly Hanekawa _client;
        private readonly InternalLogService _log;
        private readonly IServiceProvider _service;
        private static readonly ConcurrentDictionary<ulong, MemoryCache> Cooldown = new ConcurrentDictionary<ulong, MemoryCache>();
        private static readonly List<ulong> Premium = new List<ulong>();

        public MvpService(Hanekawa client, InternalLogService log, IServiceProvider service)
        {
            _client = client;
            _log = log;
            _service = service;

            //_client.MessageReceived += CountMvp;
            // Actual MVP Counting is now done in ExpService 
            // To use the same cooldown & channel ignore cache
            _client.RoleDeleted += MvpRoleCheckDeletion;
        }

        private Task MvpRoleCheckDeletion(RoleDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                using var scope = _service.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var mvpConfig = await db.MvpConfigs.FindAsync(e.Role.Guild);
                if (mvpConfig?.RoleId == null) return;
                if (e.Role.Id.RawValue != mvpConfig.RoleId.Value) return;
                mvpConfig.RoleId = null;
                await db.SaveChangesAsync();
                _log.LogAction(LogLevel.Information, "(MVP Service) Removed MVP role as it was deleted.");
            });
            return Task.CompletedTask;
        }
/*
        private Task CountMvp(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Message.Author.IsBot) return;
                if (!(e.Message.Author is CachedMember user)) return;
                if (!(await ServerCheck(user.Guild))) return;
                var userCd = Cooldown.GetOrAdd(user.Guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
                if (userCd.TryGetValue(user.Id.RawValue, out _)) return;
                userCd.Set(user.Id.RawValue, false, TimeSpan.FromMinutes(1));
                using var scope = _service.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var userData = await db.GetOrCreateUserData(user);
                userData.MvpCount++;
                await db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }
*/
        private async Task<bool> ServerCheck(CachedGuild guild)
        {
            if (Premium.Contains(guild.Id.RawValue)) return true;
            using (var scope = _service.CreateScope())
            await using (var db = scope.ServiceProvider.GetRequiredService<DbService>())
            {
                var guildCfg = await db.GetOrCreateGuildConfigAsync(guild);
                if (!guildCfg.Premium) return false;
                Premium.Add(guild.Id.RawValue);
            }
            return true;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _ = MvpReward();
            return Task.CompletedTask;
        }

        private async Task MvpReward()
        {
            using var scope = _service.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var premium = await db.GuildConfigs.Where(x => x.Premium).ToListAsync();

            for (var i = 0; i < premium.Count; i++)
            {
                var mvps = new List<CachedMember>();
                var oldMvps = new List<CachedMember>();
                try
                {
                    var x = premium[i];
                    var mvpConfig = await db.MvpConfigs.FindAsync(x.GuildId);
                    if(DateTime.UtcNow.DayOfWeek != mvpConfig.Day) continue;
                    if (mvpConfig?.RoleId != null)
                    {
                        var guild = _client.GetGuild(x.GuildId);
                        var role = guild.GetRole(mvpConfig.RoleId.Value);
                        if (role != null)
                        {
                            var toAdd = guild.Members.Where(e => e.Value.Roles.ContainsKey(role.Id)).ToList();
                            for (var j = 0; j < toAdd.Count; j++)
                            {
                                oldMvps.Add(toAdd[j].Value);
                            }
                            var users = await db.Accounts.Where(e => e.GuildId == mvpConfig.GuildId && e.Active).OrderByDescending(e => e.MvpCount).Take(mvpConfig.Count).ToListAsync();
                            for (var j = 0; j < mvpConfig.Count; j++)
                            {
                                var e = users[j];
                                var user = guild.GetMember(e.UserId);
                                await user.TryAddRoleAsync(role);
                                mvps.Add(user);
                            }
                            _log.LogAction(LogLevel.Information, $"(MVP Service) Rewarded {mvpConfig.Count} users with MVP role in {guild.Id.RawValue}");
                        }
                        else
                        {
                            mvpConfig.RoleId = null;
                            await db.SaveChangesAsync();
                            _log.LogAction(LogLevel.Information, $"(MVP Service) Reset MVP role as it was null in {guild.Id.RawValue}");
                        }
                    }
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync("UPDATE Accounts" +
                                                             "SET MvpCount = 0" +
                                                             $"WHERE GuildId = {x.GuildId}");
                    }
                    catch (Exception e)
                    {
                        await db.Accounts.ForEachAsync(z => z.MvpCount = 0);
                        _log.LogAction(LogLevel.Error, e, $"(MVP Service) Failed to execute raw SQL in {x.GuildId}");
                    }
                    await db.SaveChangesAsync();
                    _log.LogAction(LogLevel.Information, $"(MVP Service) Reset every ones MVP counter to 0 in {x.GuildId}");
                    var strb = new StringBuilder();
                    for (var j = 0; j < mvps.Count; j++)
                    {
                        var n = mvps[j];
                        var o = oldMvps[j];
                        strb.AppendLine($"{o.Mention ?? "User Left"} => {n.Mention}");
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(MVP Service) Error when assigning MVP rewards\n{e.Message}");
                }
            }
        }
    }
}
