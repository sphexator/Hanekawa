using System.Collections.Generic;

namespace HungerGame.Entities.Items
{
    public class ItemDrop
    {
        public List<Drink> Drinks { get; set; }
        public List<Food> Foods { get; set; }
        public List<FirstAid> FirstAids { get; set; }
        public List<Weapon> Weapons { get; set; }
    }
}