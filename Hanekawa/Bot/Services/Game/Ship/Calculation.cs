using System;
using System.Threading.Tasks;
using Hanekawa.Database;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Shared.Game;

namespace Hanekawa.Bot.Services.Game.Ship
{
    public partial class ShipGameService
    {
        private int DefaultHealth { get; } = 10;
        private int DefaultDamage { get; } = 1;

        private async Task<GameClass> GetClassName(int classId, DbService db) =>
            await db.GameClasses.FindAsync(classId);

        public int GetHealth(int level, GameClass ass) =>
            Convert.ToInt32(Math.Round(DefaultHealth * level * ass.ModifierHealth));

        public int GetHealth(int level, GameEnemy enemyData, GameClass enemyClass) =>
            Convert.ToInt32(Math.Round((DefaultHealth + enemyData.Health) * level *
                                       enemyClass.ModifierHealth));

        public int GetDamage(int level) => DefaultDamage * level;

        public int GetDamage(int level, GameEnemy enemyData) => (DefaultDamage + enemyData.Damage) * level;

        public int CalculateDamage(int damage, GameClass AttackerClass, GameClass EnemyClass, EnemyType type)
        {
            var avoid = new Random().Next(100);
            var crit = new Random().Next(100);
            if (type == EnemyType.Player)
                if (avoid <= EnemyClass.ChanceAvoid)
                    return 0;
            if (crit <= AttackerClass.ChanceCrit)
                damage = Convert.ToInt32(damage * AttackerClass.ModifierCriticalChance);
            var lowDmg = damage / 2;
            if (lowDmg <= 0) lowDmg = 5;
            var highDmg = damage * 2;
            if (lowDmg >= highDmg) highDmg = lowDmg + 10;
            damage = new Random().Next(lowDmg, highDmg);

            return damage;
        }
    }
}