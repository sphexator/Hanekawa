using System;
using Disqord;

namespace Hanekawa.Database.Tables.Account
{
    public class Inventory
    {
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public Guid ItemId { get; set; }
        public int Amount { get; set; } = 1;
    }
}