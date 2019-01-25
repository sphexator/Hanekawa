using System;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Services.Achievement;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Services.Level.Util;

namespace Hanekawa.Services.Level
{
    public class ExperienceHandler : IHanaService
    {
        private readonly LevelRoleHandler _roleHandler;
        private readonly AchievementManager _achievement;
        private readonly LevelGenerator _levelGenerator;
        private readonly ConcurrentDictionary<ulong, int> _expMultiplier
            = new ConcurrentDictionary<ulong, int>();

        public ExperienceHandler(LevelRoleHandler handler, AchievementManager achievement, LevelGenerator levelGenerator)
        {
            _roleHandler = handler;
            _achievement = achievement;
            _levelGenerator = levelGenerator;
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

        public async Task AddVoiceExp(DbService db, IGuildUser user, int exp, int credit, Account userdata, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (newState.VoiceChannel != null && oldState.VoiceChannel == null)
            {
                userdata.VoiceExpTime = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return;
            }

            if (oldState.VoiceChannel == null || newState.VoiceChannel != null) return;

            userdata.StatVoiceTime = userdata.StatVoiceTime + (DateTime.UtcNow - userdata.VoiceExpTime);
            userdata.Sessions = userdata.Sessions + 1;
            await AddExperience(db, user, exp, credit);
            await _achievement.AtOnce(user as SocketGuildUser, DateTime.UtcNow - userdata.VoiceExpTime);
            await _achievement.TotalTime(user as SocketGuildUser, DateTime.UtcNow - userdata.VoiceExpTime);
        }

        public async Task AddDropExp(IGuildUser user, int exp, int credit)
        {
            using (var db = new DbService())
            {
                await AddExperience(db, user, exp, credit);
                await _achievement.DropClaimed(db, user);
            }
        }
        
        public async Task AddGlobalExp(IGuildUser user, int exp, int credit)
        {
            using (var db = new DbService())
            {
                await AddExperience(db, user, exp, credit);
            }
        }

        public async Task AddGlobalExp(DbService db, IGuildUser user, int exp, int credit) =>
            await AddGlobalExperience(db, user, exp, credit);

        private async Task AddExperience(DbService db, IGuildUser user, int exp, int credit)
        {
            var userdata = await db.GetOrCreateUserData(user);
            var levelExp = _levelGenerator.GetServerLevelRequirement(userdata.Level);
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
            var levelExp = _levelGenerator.GetGlobalLevelRequirement(userdata.Level);
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

        // Event Handler
        public void AdjustMultiplier(ulong guildId, int multiplier) =>
            _expMultiplier.AddOrUpdate(guildId, multiplier, (key, value) => multiplier);

        public int GetMultiplier(ulong guildId) => _expMultiplier.GetOrAdd(guildId, 1);
    }
}
