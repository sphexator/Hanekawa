using System;

namespace Hanekawa.Entities.Game.ShipGame
{
    public class GameConfig
    {
        public Guid Id { get; set; }
        public int DefaultHealth { get; set; } = 10;
        public int DefaultDamage { get; set; } = 1;
    }
}