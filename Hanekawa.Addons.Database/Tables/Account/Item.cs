using System;

namespace Hanekawa.Database.Tables.Account
{
    public class Item
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong Role { get; set; }
        public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
    }
}