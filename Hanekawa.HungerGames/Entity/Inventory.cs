using System.Collections.Generic;

namespace Hanekawa.HungerGames.Entity
{
    public class Inventory
    {
        public int Id { get; set; }
        public List<Item> Items { get; set; }
    }
}