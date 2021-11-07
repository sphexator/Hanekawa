namespace Hanekawa.Entities.Account.Stores
{
    public class ServerStore
    {
        public ServerStore() { }
        public ServerStore(ulong guildId, ulong roleId)
        {
            GuildId = guildId;
            RoleId = roleId;
        }
        
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public int Price { get; set; }
        public bool SpecialCredit { get; set; }
    }
}