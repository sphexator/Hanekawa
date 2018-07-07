namespace Jibril.Services.Entities.Tables
{
    public class Inventory
    {
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public uint Amount { get; set; }
        public bool Unique { get; set; }
        public bool Consumable { get; set; }
    }
}