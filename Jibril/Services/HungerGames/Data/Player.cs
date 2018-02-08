using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.HungerGames.Data
{
    public class Player
    {
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Damage { get; set; }
        public int Hunger { get; set; }
        public int Sleep { get; set; }
        public bool Status { get; set; }
    }

    public class Weapons
    {
        public int Pistol { get; set; }
        public int Bullets { get; set; }
        public int Bow { get; set; }
        public int Arrows { get; set; }
        public int Axe { get; set; }
        public int Trap { get; set; }
    }

    public class Consumables
    {
        public int Beans { get; set; }
        public int Pasta { get; set; }
        public int Fish { get; set; }
        public int Ramen { get; set; }
        public int Coke { get; set; }
        public int Water { get; set; }
        public int MountainDew { get; set; }
        public int Redbull { get; set; }
    }
}
