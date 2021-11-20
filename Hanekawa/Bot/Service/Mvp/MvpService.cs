using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Premium;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Hanekawa.Bot.Service.Mvp
{
    public class MvpService : INService, IJob
    {
        private readonly Hanekawa _bot;
        private readonly ILogger<MvpService> _logger;
        private readonly IServiceProvider _provider;

        public MvpService(Hanekawa bot, IServiceProvider provider, ILogger<MvpService> logger)
        {
            _bot = bot;
            _provider = provider;
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _ = MvpReward();
            return Task.CompletedTask;
        }

        public async Task MvpReward()
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var premium = await db.GuildConfigs
                .Where(x => x.Premium.HasValue && x.Premium.Value >= DateTimeOffset.UtcNow).ToListAsync();

            foreach (var x in premium) await RewardAsync(x, db);
        }

        public async Task RewardAsync(GuildConfig x, DbService db, bool bypass = false)
        {
            var mvp = new List<IMember>();
            var oldMvp = new List<IMember>();

            var mvpConfig = await db.MvpConfigs.FindAsync(x.GuildId);
            if (mvpConfig == null) return;
            if (mvpConfig.Disabled) return;
            if (DateTime.UtcNow.DayOfWeek != mvpConfig.Day && !bypass) return;

            var guild = _bot.GetGuild(x.GuildId);
            if (guild == null) return;
            try
            {
                if (mvpConfig.RoleId.HasValue)
                    if (!await RoleHandlingAsync(db, guild, mvpConfig, oldMvp, mvp))
                        return;
                try
                {
                    await db.Database.ExecuteSqlRawAsync("UPDATE Accounts " +
                                                         "SET MvpCount = 0 " +
                                                         $"WHERE GuildId = '{x.GuildId}'");
                }
                catch (Exception e)
                {
                    await db.Accounts.ForEachAsync(z => z.MvpCount = 0);
                    _logger.LogError(e, "Failed to execute raw SQL in {GuildId}", x.GuildId);
                }

                await db.SaveChangesAsync();
                _logger.LogInformation("Reset every ones MVP counter to 0 in {GuildId}", x.GuildId);

                await PostAsync(guild, mvpConfig, mvp, oldMvp);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when assigning MVP rewards in {GuildId}\n{ExceptionMessage}", 
                    guild.Id, e.Message);
            }
        }

        private async Task<bool> RoleHandlingAsync(DbService db, IGatewayGuild guild, MvpConfig mvpConfig,
            ICollection<IMember> oldMvp, ICollection<IMember> mvp)
        {
            if (!mvpConfig.RoleId.HasValue) return false;
            if (!guild.Roles.TryGetValue(mvpConfig.RoleId.Value, out var role))
            {
                mvpConfig.RoleId = null;
                await db.SaveChangesAsync();
                _logger.LogInformation("Reset MVP role as it was null in {GuildId}", guild.Id);
                return false;
            }

            var toAdd = guild.Members.Where(e => e.Value.GetRoles().ContainsKey(role.Id)).ToList();
            if (toAdd.Count > 0)
                foreach (var (key, member) in toAdd)
                    try
                    {
                        await member.TryRemoveRoleAsync(role);
                        oldMvp.Add(member);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Couldn't remove {MvpRole} role from {UserId} in {GuildId}", 
                            role.Id, key, member.GuildId);
                    }

            var users = await db.Accounts.Where(e => e.GuildId == mvpConfig.GuildId && e.Active && !e.MvpOptOut)
                .OrderByDescending(e => e.MvpCount).Take(mvpConfig.Count * 2).ToListAsync();
            for (var j = 0; j < mvpConfig.Count; j++)
            {
                if (users.Count < mvpConfig.Count && j >= users.Count) continue;
                var e = users[j];
                try
                {
                    var user = await guild.GetOrFetchMemberAsync(e.UserId);
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
                    _logger.LogError(exception, "Couldn't add {MvpRole} role to {UserId} in {GuildId}", 
                        role.Id, e.UserId, e.GuildId);
                }
            }

            _logger.LogInformation("Rewarded {MvpCount} users with MVP role in {GuildId}", mvpConfig.Count, guild.Id);
            return true;
        }

        private async Task PostAsync(CachedGuild guild, MvpConfig cfg, IReadOnlyList<IMember> mvp,
            IReadOnlyList<IMember> oldMvp)
        {
            if (!cfg.ChannelId.HasValue) return;
            try
            {
                var channel = guild.GetChannel(cfg.ChannelId.Value);
                if (channel == null)
                {
                    _logger.LogError("(MVP Service) Couldn't find announcement channel");
                    return;
                }

                var textChannel = channel as ITextChannel;
                var stringBuilder = new StringBuilder();
                for (var j = 0; j < mvp.Count; j++)
                {
                    IMember oldMvpUser = null;
                    IMember newMvpUser = null;
                    newMvpUser = mvp[j];
                    if (j < oldMvp.Count) oldMvpUser = oldMvp[j];

                    stringBuilder.AppendLine($"{oldMvpUser?.Name ?? "User Left"} => {newMvpUser.Name}");
                }

                await textChannel.SendMessageAsync(new LocalMessage
                {
                    Attachments = null,
                    Content = $"New Weekly MVP!\n {stringBuilder}",
                    Embeds = null,
                    AllowedMentions = LocalAllowedMentions.None,
                    IsTextToSpeech = false,
                    Reference = null
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Couldn't send message in guild {GuildId} for channel {MvpChannel}", cfg.GuildId, cfg.ChannelId.Value);
            }
        }
    }
}