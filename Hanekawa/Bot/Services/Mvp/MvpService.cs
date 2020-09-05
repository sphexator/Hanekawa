using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        public MvpService(Hanekawa client, InternalLogService log, IServiceProvider service)
        {
            _client = client;
            _log = log;
            _service = service;

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
                var x = premium[i];
                await Reward(x, db);
            }
        }

        public async Task Reward(GuildConfig x, DbService db, bool bypass = false)
        {
            var mvp = new List<CachedMember>();
            var oldMvp = new List<CachedMember>();

            var mvpConfig = await db.MvpConfigs.FindAsync(x.GuildId);
            if (mvpConfig == null) return;
            if (mvpConfig.Disabled) return;
            if (DateTime.UtcNow.DayOfWeek != mvpConfig.Day && !bypass) return;

            var guild = _client.GetGuild(x.GuildId);
            if (guild == null) return;
            try
            {
                if (mvpConfig.RoleId != null)
                {
                    var role = guild.GetRole(mvpConfig.RoleId.Value);
                    if (role != null)
                    {
                        var toAdd = guild.Members.Where(e => e.Value.Roles.ContainsKey(role.Id)).ToList();
                        if (toAdd.Count > 0)
                        {
                            for (var j = 0; j < toAdd.Count; j++)
                            {
                                try
                                {
                                    await toAdd[j].Value.TryRemoveRoleAsync(role);
                                    oldMvp.Add(toAdd[j].Value);
                                }
                                catch (Exception e)
                                {
                                    _log.LogAction(LogLevel.Error, e, $"(MVP Service) Couldn't remove role from {toAdd[j].Key}");
                                }
                            }
                        }

                        var users = await db.Accounts.Where(e => e.GuildId == mvpConfig.GuildId && e.Active)
                            .OrderByDescending(e => e.MvpCount).Take(mvpConfig.Count * 2).ToListAsync();
                        for (var j = 0; j < mvpConfig.Count; j++)
                        {
                            if (users.Count < mvpConfig.Count && j >= users.Count) continue;
                            var e = users[j];
                            try
                            {
                                var user = guild.GetMember(e.UserId);
                                if (user == null)
                                {
                                    j--;
                                    continue;
                                }
                                await user.TryAddRoleAsync(role);
                                mvp.Add(user);
                            }
                            catch (Exception exception)
                            {
                                _log.LogAction(LogLevel.Error, exception, $"(MVP Service) Couldn't add role to {e.UserId}");
                            }
                        }

                        _log.LogAction(LogLevel.Information,
                            $"(MVP Service) Rewarded {mvpConfig.Count} users with MVP role in {guild.Id.RawValue}");
                    }
                    else
                    {
                        mvpConfig.RoleId = null;
                        await db.SaveChangesAsync();
                        _log.LogAction(LogLevel.Information,
                            $"(MVP Service) Reset MVP role as it was null in {guild.Id.RawValue}");
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

                var guildConfig = await db.GetOrCreateGuildConfigAsync(guild);
                if (guildConfig.MvpChannel.HasValue)
                {
                    try
                    {
                        var channel = guild.GetTextChannel(guildConfig.MvpChannel.Value);
                        if (channel == null)
                        {
                            _log.LogAction(LogLevel.Warning, "(MVP Service) Couldn't find announcement channel");
                            return;
                        }

                        var stringBuilder = new StringBuilder();
                        for (var j = 0; j < mvp.Count; j++)
                        {
                            CachedMember o = null;
                            CachedMember n = null;
                            n = mvp[j];
                            if (j < oldMvp.Count)
                            {
                                o = oldMvp[j];
                            }
                            stringBuilder.AppendLine($"{o?.Mention ?? "User Left"} => {n.Mention}");
                        }

                        await channel.SendMessageAsync(null, false, new LocalEmbedBuilder().Create(
                            "New Weekly MVP!\n" +
                            $"{stringBuilder}", Color.Green).Build());
                    }
                    catch (Exception e)
                    {
                        _log.LogAction(LogLevel.Error, e,
                            $"(MVP Service) Couldn't send message for guild {guildConfig.GuildId} in channel {guildConfig.MvpChannel.Value}");
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(MVP Service) Error when assigning MVP rewards\n{e.Message}");
            }
        }
    }
}
