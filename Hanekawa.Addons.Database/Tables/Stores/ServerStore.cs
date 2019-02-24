namespace Hanekawa.Addons.Database.Tables.Stores
{
    public class ServerStore
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public int Price { get; set; }
        public bool SpecialCredit { get; set; }
    }
}