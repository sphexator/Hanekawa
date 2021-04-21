﻿using Disqord;
using Hanekawa.Database.Tables.Account.ShipGame;

namespace Hanekawa.Entities
{
    public record ShipUser
    {
        public ShipUser(GameEnemy enemy, int level, GameClass gameClass, int damage, int health)
        {
            Id = enemy.Id;
            Name = enemy.Name;
            Level = level;
            Avatar = enemy.ImageUrl;

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

        public ShipUser(IMember user, int level, GameClass gameClass, int damage, int health)
        {
            Id = user.Id.RawValue;
            Name = user.Nick ?? user.Name;
            Level = level;
            Avatar = user.GetAvatarUrl();

            MaxHealth = health;
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
        public Snowflake Id { get; set; }
        public int Level { get; set; }
        public string Avatar { get; set; }

        public int MaxHealth { get; init; }
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