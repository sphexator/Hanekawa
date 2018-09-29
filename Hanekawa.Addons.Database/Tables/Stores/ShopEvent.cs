namespace Hanekawa.Addons.Database.Tables
{
    public class ShopEvent
    {
        public uint Id { get; set; }
        public string Item { get; set; }
        public uint Price { get; set; }
        public uint? Stock { get; set; }
    }
}