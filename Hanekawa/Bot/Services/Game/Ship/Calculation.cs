using System;
using System.Threading.Tasks;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Shared.Game;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService
    {
        private int DefaultHealth { get; } = 10;
        private int DefaultDamage { get; } = 1;

        private async Task<GameClass> GetClass(int classId, DbService db) =>
            await db.GameClasses.FindAsync(classId);

        private int GetHealth(int level, GameClass ass) =>
            Convert.ToInt32(Math.Round(DefaultHealth * level * ass.ModifierHealth));

        private int GetHealth(int level, GameEnemy enemyData, GameClass enemyClass) =>
            Convert.ToInt32(Math.Round((DefaultHealth + enemyData.Health) * level *
                                       enemyClass.ModifierHealth));

        private int GetDamage(int level) => DefaultDamage * level;

        private int GetDamage(int level, GameEnemy enemyData) => (DefaultDamage + enemyData.Damage) * level;

        private int CalculateDamage(int damage, GameClass attackerClass, GameClass enemyClass, EnemyType type)
        {
            var avoid = _random.Next(100);
            var crit = _random.Next(100);
            if (type == EnemyType.Player)
                if (avoid <= enemyClass.ChanceAvoid)
                    return 0;
            if (crit <= attackerClass.ChanceCrit)
                damage = Convert.ToInt32(damage * attackerClass.ModifierCriticalChance);
            var lowDmg = damage / 2;
            if (lowDmg <= 0) lowDmg = 5;
            var highDmg = damage * 2;
            if (lowDmg >= highDmg) highDmg = lowDmg + 10;
            damage = new Random().Next(lowDmg, highDmg);

            return damage;
        }
/*
        private async Task<int> Health(DbService db, int level, SocketGuildUser user)
        {
            var userdata = await db.GetOrCreateUserData(user);
            return _gameStats.GetHealth(level, await GetClass(db, userdata.Class));
        }

        private async Task<int> Health(DbService db, int level, GameEnemy enemy) =>
            _gameStats.GetHealth(level, enemy, await GetClass(db, enemy.ClassId));

        private int Damage(int level) => _gameStats.GetDamage(level);

        private int Damage(int level, GameEnemy enemy) => _gameStats.GetDamage(level, enemy);
        */
    }
}