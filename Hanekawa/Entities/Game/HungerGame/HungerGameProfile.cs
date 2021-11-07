using System;

namespace Hanekawa.Entities.Game.HungerGame
{
    public class HungerGameProfile
    {
        public HungerGameProfile() { }
        public HungerGameProfile(ulong guildId, ulong userId)
        {
            GuildId = guildId;
            UserId = userId;
        }
        
        public Guid Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public bool Bot { get; set; } = false;
        
        public string Name { get; set; } = "Test Unit";
        public string Avatar { get; set; } = null;
        
        public bool Alive { get; set; } = true;
        public double Health { get; set; } = 100;
        public double Stamina { get; set; } = 100;
        public bool Bleeding { get; set; } = false;

        public double Hunger { get; set; } = 100;
        public double Thirst { get; set; } = 100;
        public double Tiredness { get; set; } = 0;
        public ActionType LastMove { get; set; } = ActionType.None;

        public int Food { get; set; } = 0;
        public int Water { get; set; } = 0;
        public int FirstAid { get; set; } = 0;
        public int Weapons { get; set; } = 0;
        public int MeleeWeapon { get; set; } = 0;
        public int RangeWeapon { get; set; } = 0;
        public int Bullets { get; set; } = 0;
        
        public int Attack()
        {

            return 0;
        }

        public void Move()
        {
            
        }

        public void Fatigue()
        {
            if (Tiredness + 10 >= 100) Tiredness = 100;
            else Tiredness += 10;
            
            if(Hunger + 5 >= 100) Hunger = 100;
            else Hunger += 5;
            
            if(Thirst + 5 >= 100) Thirst = 100;
            else Thirst += 5;
        }

        public void Bleed()
        {
            if(Bleeding) Health -= 10;
        }
        
        public void Eat()
        {
            Hunger = 0;
            Heal(10);
        }
        
        public void Drink()
        {
            Thirst = 0;
        }
        
        public void Rest()
        {
            Tiredness = 0;
        }
        
        public void Heal(int amount, bool firstAid = false)
        {
            if(Health + amount >= 100) Health = 100;
            else Health += amount;
            
            if(firstAid && Bleeding) Bleeding = false;
        }
    }
}