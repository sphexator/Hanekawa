using System;
using Disqord;
using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class Participants
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "Test";
        public Snowflake UserId { get; set; }
        public string AvatarUrl { get; set; } = null;
        public bool Bot { get; set; } = false;
        public int Health { get; set; } = 100;
        public int Stamina { get; set; } = 100;
        public bool Bleeding { get; set; } = false;
        
        public double Hunger { get; set; } = 100;
        public double Thirst { get; set; } = 100;
        public double Sleep { get; set; } = 0;
        
        public ActionType LastMove { get; set; } = ActionType.None;

        public int Food { get; set; } = 0;
        public int Water { get; set; } = 0;
        public int FirstAid { get; set; } = 0;
        public int Weapons { get; set; } = 0;
        public int MeleeWeapon { get; set; } = 0;
        public int RangeWeapon { get; set; } = 0;
        public int Bullets { get; set; } = 0;
        
        public Guid GameId { get; set; }
        public Game Game { get; set; }
    }
}