using System;
using System.Threading.Tasks;
using Hanekawa.Core.Game;
using Hanekawa.Database;
using Hanekawa.Database.Tables.BotGame;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService
    {
        private int DefaultHealth { get; set; } = 10;
        private int DefaultDamage { get; set; } = 1;
        private async Task<GameClass> GetClassName(int classId, DbService db) => await db.GameClasses.FindAsync(classId);

        private int GetHealth(int level, GameClass gameClass) =>
            Convert.ToInt32(Math.Round(DefaultHealth * level * gameClass.ModifierHealth));

        private int GetHealth(int level, GameEnemy enemyData, GameClass enemyClass) =>
            Convert.ToInt32(Math.Round((DefaultHealth + enemyData.Health) * level *
                                       enemyClass.ModifierHealth));

        public int GetDamage(int level) => DefaultDamage * level;
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

            return _random.Next(lowDmg, highDmg);
        }
    }
}