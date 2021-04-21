using System;
using Disqord;
using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Account
{
    public class Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Sell { get; set; }
        public ItemType Type { get; set; } = ItemType.Trash;

        public Snowflake? GuildId { get; set; } = null;
        public Snowflake? Role { get; set; } = null;

        public int HealthIncrease { get; set; } = 0;
        public int DamageIncrease { get; set; } = 0;
        public int CriticalIncrease { get; set; } = 0;

        public string ImageUrl { get; set; }

        public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
    }
}