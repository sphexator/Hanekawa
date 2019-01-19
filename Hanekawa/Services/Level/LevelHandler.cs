using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Hanekawa.Services.Achievement;
using Hanekawa.Services.Level.Util;

namespace Hanekawa.Services.Level
{
    public class LevelHandler : IHanaService
    {
        private readonly LevelRoleHandler _roleHandler;
        private readonly Calculate _calculate;
        private readonly AchievementManager _achievement;
        private readonly ConcurrentDictionary<ulong, int> _expMultiplier
            = new ConcurrentDictionary<ulong, int>();

        public LevelHandler(LevelRoleHandler roleHandler, Calculate calculate, AchievementManager achievement)
        {
            _roleHandler = roleHandler;
            _calculate = calculate;
            _achievement = achievement;
        }

        public async Task AddExp(IGuildUser user, int exp, int credit)
        {
            using (var db = new DbService())
            {
                await AddExperience(db, user, exp, credit);
            }
        }

        public async Task AddExp(DbService db, IGuildUser user, int exp, int credit) =>
            await AddExperience(db, user, exp, credit);

        public async Task AddGlobalExp(IGuildUser user, int exp, int credit)
        {
            using (var db = new DbService())
            {
                await AddExperience(db, user, exp, credit);
            }
        }

        public async Task AddGlobalExp(DbService db, IGuildUser user, int exp, int credit) =>
            await AddGlobalExperience(db, user, exp, credit);

        public void AdjustMultiplier(ulong guildId, int multiplier) => _expMultiplier.AddOrUpdate(guildId, multiplier, (key, value) => multiplier);

        public int GetMultiplier(ulong guildId) => _expMultiplier.GetOrAdd(guildId, 1);

        private async Task AddExperience(DbService db, IGuildUser user, int exp, int credit)
        {
            var userdata = await db.GetOrCreateUserData(user);
            var levelExp = _calculate.GetServerLevelRequirement(userdata.Level);
            exp = exp * _expMultiplier.GetOrAdd(user.GuildId, 1);

            userdata.TotalExp = userdata.TotalExp + exp;
            userdata.Credit = userdata.Credit + credit;
            if (userdata.Exp + exp >= levelExp)
            {
                userdata.Level += 1;
                userdata.Exp = userdata.Exp + exp - levelExp;
                await _roleHandler.NewLevelManagerAsync(db, userdata, user);
                await _achievement.ServerLevelAchievement(user, userdata);
            }
            else
            {
                userdata.Exp += exp;
            }

            await db.SaveChangesAsync();
        }

        private async Task AddGlobalExperience(DbService db, IGuildUser user, int exp, int credit)
        {
            var userdata = await db.GetOrCreateGlobalUserData(user);
            var levelExp = _calculate.GetGlobalLevelRequirement(userdata.Level);
            userdata.TotalExp += exp;
            userdata.Credit += credit;
            if (userdata.Exp + exp >= levelExp)
            {
                userdata.Level += 1;
                userdata.Exp = userdata.Exp + exp - levelExp;
                await _achievement.GlobalLevelAchievement(user, userdata);
            }
            else
            {
                userdata.Exp += exp;
            }

            await db.SaveChangesAsync();
        }
    }
}