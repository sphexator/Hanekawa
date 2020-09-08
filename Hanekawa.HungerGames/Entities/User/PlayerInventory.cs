using System.Collections.Generic;
using Hanekawa.HungerGames.Entities.User.Items;

namespace Hanekawa.HungerGames.Entities.User
{
    public class PlayerInventory
    {
        public List<WeaponInventory> Weapons { get; set; }
        public List<DrinkInventory> Drinks { get; set; }
        public List<FoodInventory> Food { get; set; }
        public List<FirstAidInventory> FirstAid { get; set; }
    }
}