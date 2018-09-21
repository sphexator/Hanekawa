namespace Hanekawa.Services.Entities.Tables
{
    public class Inventory
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public uint ItemId { get; set; }
        public uint Amount { get; set; }
    }
}