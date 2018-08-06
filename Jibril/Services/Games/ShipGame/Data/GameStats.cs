using System;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;

namespace Jibril.Services.Games.ShipGame.Data
{
    public class GameStats
    {
        public GameStats()
        {
            using (var db = new DbService())
            {
                var cfg = db.GameConfigs.Find(1);
                DefaultHealth = cfg.DefaultHealth;
                DefaultDamage = cfg.DefaultDamage;
            }
        }

        private int DefaultHealth { get; }
        private int DefaultDamage { get; }

        public int GetHealth(uint level, GameClass ass)
        {
            return Convert.ToInt32(Math.Round(DefaultHealth * level * ass.ModifierHealth));
        }

        public int GetHealth(uint level, GameEnemy enemyData, GameClass enemyClass)
        {
            return Convert.ToInt32(Math.Round((DefaultHealth + enemyData.Health) * level *
                                              enemyClass.ModifierHealth));
        }

        public int GetDamage(uint level)
        {
            return Convert.ToInt32(DefaultDamage * level);
        }

        public int GetDamage(uint level, GameEnemy enemyData)
        {
            return Convert.ToInt32((DefaultDamage + enemyData.Damage) * level);
        }

        public int CalculateDamage(int damage, GameClass ass)
        {
            var avoid = new Random().Next(100);
            var crit = new Random().Next(100);
            if (avoid <= ass.ChanceAvoid) return 0;
            if (crit <= ass.ChanceCrit) damage = Convert.ToInt32(damage * ass.ModifierCriticalChance);
            var lowDmg = damage / 2;
            if (lowDmg <= 0) lowDmg = 5;
            var highDmg = damage * 2;
            if (lowDmg >= highDmg) highDmg = lowDmg + 10;
            damage = new Random().Next(lowDmg, highDmg);

            return damage;
        }
    }
}