using System;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            _ = DecayAsync();
            return Task.CompletedTask;
        }

        private async Task DecayAsync()
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var limit = DateTime.UtcNow.AddDays(-14);
                var servers = await db.LevelConfigs.Where(x => x.Decay).ToArrayAsync();
                if (servers.Length <= 0) return;
                foreach (var x in servers)
                {
                    var server = _client.GetGuild(x.GuildId);
                    if (server == null) continue;
                    var levelRewards = await db.LevelRewards.Where(e => e.GuildId == x.GuildId).ToListAsync();
                    var users = await db.Accounts.Where(e =>
                        e.GuildId == x.GuildId && e.Active && e.LastMessage <= limit && e.Decay < e.TotalExp).ToArrayAsync();
                    foreach (var user in users)
                    {
                        if (user.TotalExp == user.Decay) continue;
                        var member = server.GetMember(user.UserId);
                        if (member == null)
                        {
                            user.Active = false;
                            continue;
                        }
                        if (member.IsBoosting) continue;
                        var decay = Convert.ToInt32((limit - user.LastMessage).TotalDays) * 1000;
                        if (decay == user.Decay) continue;
                        if (user.TotalExp <= user.Decay + decay) user.Decay = user.TotalExp;
                        if (user.TotalExp > user.Decay + decay) user.Decay = decay;
                        await RoleCheckAsync(member, x, user, levelRewards, db, LevelDecay(user, decay));
                    }
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Exp Service: Decay) Error occured during decay - {e.Message}");
            }
        }

        private int LevelDecay(Account userData, int decay)
        {
            var decayExp = decay;
            var decayLevel = 0;
            var currentExp = userData.Exp;
            while (decayExp > 0)
            {
                if (currentExp - decay <= 0)
                {
                    decayExp =- currentExp;
                    currentExp = ExpToNextLevel((userData.Level - decayLevel) - 1);
                    decayLevel++;
                    continue;
                } 
                if (currentExp - decay > 0) decay = 0;
            }

            return decayLevel;
        }
    }
}