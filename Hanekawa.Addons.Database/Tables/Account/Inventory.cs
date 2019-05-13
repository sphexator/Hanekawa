namespace Hanekawa.Addons.Database.Tables.Account
{
    public class Inventory
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int ItemId { get; set; }
        public int Amount { get; set; } = 1;
    }
}