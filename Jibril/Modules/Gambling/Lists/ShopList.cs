using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Modules.Gambling.Lists
{
    public class ShopList
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public int Price { get; set; }
    }
    public class EventShopList
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }
    }
}
