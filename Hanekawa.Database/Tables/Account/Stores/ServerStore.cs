using Disqord;

namespace Hanekawa.Database.Tables.Account.Stores
{
    public class ServerStore
    {
        public ServerStore() { }
        public ServerStore(Snowflake guildId, Snowflake roleId)
        {
            GuildId = guildId;
            RoleId = roleId;
        }
        
        public Snowflake GuildId { get; set; }
        public Snowflake RoleId { get; set; }
        public int Price { get; set; }
        public bool SpecialCredit { get; set; }
    }
}