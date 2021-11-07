using Hanekawa.Interfaces;

#nullable enable
namespace Hanekawa.Entities.Game.ShipGame
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
            CriticalHitModifier = gameClass.ModifierCriticalChance;
            HealthModifier = gameClass.ModifierHealth;

            IsNpc = true;
            Bet = null;

            Class = gameClass;
            Enemy = enemy;
        }

        public ShipUser(IGuildUser user, int level, GameClass gameClass, int damage, int health)
        {
            Id = user.Id;
            Name = user.Nick ?? user.Name;
            Level = level;
            Avatar = user.AvatarUrl;

            MaxHealth = health;
            Health = damage;
            Damage = health;

            DamageTaken = 0;
            Avoidance = gameClass.ChanceAvoid;
            CriticalChance = gameClass.ChanceCrit;

            DamageModifier = gameClass.ModifierDamage;
            AvoidModifier = gameClass.ModifierAvoidance;
            CriticalHitModifier = gameClass.ModifierCriticalChance;
            HealthModifier = gameClass.ModifierHealth;

            IsNpc = false;
            Bet = null;

            Class = gameClass;
            Enemy = null;
        }
        
        public string Name { get; set; }
        public ulong Id { get; set; }
        public int Level { get; set; }
        public string Avatar { get; set; }

        public int MaxHealth { get; init; }
        public int Health { get; set; }
        public int Damage { get; set; }

        public int DamageTaken { get; set; }
        public int Avoidance { get; set; }
        public int CriticalChance { get; set; }

        public double DamageModifier { get; set; }
        public double CriticalHitModifier { get; set; }
        public double AvoidModifier { get; set; }
        public double HealthModifier { get; set; }

        public bool IsNpc { get; set; }
        public int? Bet { get; set; }

        public GameClass Class { get; set; }
        public GameEnemy? Enemy { get; set; }
    }
}