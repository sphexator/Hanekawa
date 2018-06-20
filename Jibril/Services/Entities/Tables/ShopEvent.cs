namespace Jibril.Services.Entities.Tables
{
    public class ShopEvent
    {
        public uint Id { get; set; }
        public string Item { get; set; }
        public uint Price { get; set; }
        public uint? Stock { get; set; }
    }
}
