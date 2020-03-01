using Discord.WebSocket;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Extensions;

namespace Hanekawa.Models
{
    public class ShipGameUser
    {
        public ShipGameUser(GameEnemy enemy, int level, GameClass gameClass, int damage, int health)
        {
            Id = (ulong)enemy.Id;
            Name = enemy.Name;
            Level = level;

            Health = damage;
            Damage = health;

            DamageTaken = 0;
            Avoidance = gameClass.ChanceAvoid;
            CriticalChance = gameClass.ChanceCrit;

            DamageModifier = gameClass.ModifierDamage;
            AvoidModifier = gameClass.ModifierAvoidance;
            CritModifier = gameClass.ModifierCriticalChance;
            HealthModifier = gameClass.ModifierHealth;

            IsNpc = true;
            Bet = null;

            Class = gameClass;
            Enemy = enemy;
        }

        public ShipGameUser(SocketGuildUser userOne, int level, GameClass gameClass, int damage, int health)
        {
            Id = userOne.Id;
            Name = userOne.GetName();
            Level = level;

            Health = damage;
            Damage = health;

            DamageTaken = 0;
            Avoidance = gameClass.ChanceAvoid;
            CriticalChance = gameClass.ChanceCrit;

            DamageModifier = gameClass.ModifierDamage;
            AvoidModifier = gameClass.ModifierAvoidance;
            CritModifier = gameClass.ModifierCriticalChance;
            HealthModifier = gameClass.ModifierHealth;

            IsNpc = false;
            Bet = null;

            Class = gameClass;
            Enemy = null;
        }

        public string Name { get; set; }
        public ulong Id { get; set; }
        public int Level { get; set; }

        public int Health { get; set; }
        public int Damage { get; set; }

        public int DamageTaken { get; set; }
        public int Avoidance { get; set; }
        public int CriticalChance { get; set; }

        public double DamageModifier { get; set; }
        public double CritModifier { get; set; }
        public double AvoidModifier { get; set; }
        public double HealthModifier { get; set; }

        public bool IsNpc { get; set; }
        public int? Bet { get; set; }

        public GameClass Class { get; set; }
        public GameEnemy? Enemy { get; set; }
    }
}