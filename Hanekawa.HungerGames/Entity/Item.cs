using System.Collections.Generic;

namespace Hanekawa.HungerGames.Entity
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; } = 1;
        public int GiveOrTake { get; set; } = 0;
        public int Ammo { get; set; } = 0;
        public bool BleedEffect { get; set; } = false;
        public bool HealOverTime { get; set; } = false;
        public ItemType Type { get; set; }

        public List<Inventory> Inventories { get; set; }
    }
}