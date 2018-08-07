namespace Hanekawa.Services.Entities.Tables
{
    public class Shop
    {
        public uint Id { get; set; }
        public string Item { get; set; }
        public uint Price { get; set; }
        public ulong? RoleId { get; set; }
        public string ROle { get; set; }
    }
}
