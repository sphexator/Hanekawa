using Disqord;

namespace Hanekawa.Database.Tables.Account.Stores
{
    public class ServerStore
    {
        public Snowflake GuildId { get; set; }
        public Snowflake RoleId { get; set; }
        public int Price { get; set; }
        public bool SpecialCredit { get; set; }
    }
}