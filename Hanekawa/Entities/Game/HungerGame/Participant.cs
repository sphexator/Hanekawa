using System;
using System.Collections.Generic;
using System.Linq;

namespace Hanekawa.Entities.Game.HungerGame
{
    public class Participant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "Test";
        public ulong UserId { get; set; }
        public string AvatarUrl { get; set; } = null;
        public bool Bot { get; set; } = false;
        public int Health { get; set; } = 100;
        public int Stamina { get; set; } = 100;
        public bool Bleeding { get; set; } = false;
        
        public double Hunger { get; set; } = 100;
        public double Thirst { get; set; } = 100;
        public double Sleep { get; set; } = 0;
        
        public int XPos { get; set; }
        public int YPos { get; set; }
        
        public ActionType LastMove { get; set; } = ActionType.None;

        public int Food { get; set; } = 0;
        public int Water { get; set; } = 0;
        public int FirstAid { get; set; } = 0;

        private int Weapons
        {
            get => MeleeWeapon + RangeWeapon;
            set { }
        }

        public int MeleeWeapon { get; set; } = 0;
        public int RangeWeapon { get; set; } = 0;
        public int Bullets { get; set; } = 0;
        
        public Guid GameId { get; set; }
        public Game Game { get; set; }

        public (Participant target, int damage) Attack(List<Participant> participants, Game game)
        {
            Participant target = null;
            var targets = participants.Where(x => x.Health < 0).ToList();
            if (targets.Count <= 1) return (null, 0); 
            var targetAreaX = XPos;
            var targetAreaY = YPos;
            while (target == null)
            {
                target = GetTarget(targets, targetAreaY, targetAreaX);
                if (targetAreaX == 5 && targetAreaY == 5) return (null, 0);
                if (targetAreaX + 1 <= game.MaxXPosition && targetAreaY + 1 <= game.MaxYPosition) return (null, 0);
                if (targetAreaX + 1 <= game.MaxXPosition) targetAreaX++;
                if (targetAreaY + 1 <= game.MaxYPosition)targetAreaY++;
            }
            return (null, 0);
        }
        
        private Participant GetTarget(List<Participant> participants, int yPos, int xPos) 
            => participants.FirstOrDefault(x => x.YPos == yPos && x.XPos == xPos && x.Id != Id);

        public void Move()
        {
            
        }

        public void Fatigue()
        {
            if (Sleep + 10 >= 100) Sleep = 100;
            else Sleep += 10;
            
            if(Hunger + 5 >= 100) Hunger = 100;
            else Hunger += 5;
            
            if(Thirst + 5 >= 100) Thirst = 100;
            else Thirst += 5;
            Bleed();
        }

        private void Bleed()
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
            Sleep = 0;
        }
        
        public void Heal(int amount, bool firstAid = false)
        {
            if(Health + amount >= 100) Health = 100;
            else Health += amount;
            
            if(firstAid && Bleeding) Bleeding = false;
        }
    }
    
}