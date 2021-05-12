using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.Account
{
    public class Inventory
    {
        public Snowflake UserId { get; set; }
        public List<Item> Items { get; set; }
    }
}