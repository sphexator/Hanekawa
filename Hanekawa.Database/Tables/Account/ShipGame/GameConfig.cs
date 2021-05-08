using System;

namespace Hanekawa.Database.Tables.Account.ShipGame
{
    public class GameConfig
    {
        public Guid Id { get; set; }
        public int DefaultHealth { get; set; } = 10;
        public int DefaultDamage { get; set; } = 1;
    }
}