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

        public int GetHealth(Account userData, GameClass ass)
        {
            return Convert.ToInt32(Math.Round(DefaultHealth * userData.Level * ass.ModifierHealth));
        }

        public int GetHealth(Account userData, GameEnemy enemyData, GameClass enemyClass)
        {
            return Convert.ToInt32(Math.Round((DefaultHealth + enemyData.Health) * userData.Level *
                                              enemyClass.ModifierHealth));
        }

        public int GetDamage(Account userData)
        {
            return Convert.ToInt32(DefaultDamage * userData.Level);
        }

        public int GetDamage(Account userData, GameEnemy enemyData)
        {
            return Convert.ToInt32((DefaultDamage + enemyData.Damage) * userData.Level);
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
            damage = new Random().Next(lowDmg, highDmg);

            return damage;
        }
    }
}