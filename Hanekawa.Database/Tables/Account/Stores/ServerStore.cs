namespace Hanekawa.Database.Tables.Account.Stores
{
    public class ServerStore
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public int Price { get; set; }
        public bool SpecialCredit { get; set; }
    }
}