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
}
