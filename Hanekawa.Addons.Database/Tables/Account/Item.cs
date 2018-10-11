using System;

namespace Hanekawa.Addons.Database.Tables.Account
{
    public class Item
    {
        public int ItemId { get; set; }
        public ulong? GuildId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Unique { get; set; }
        public bool Global { get; set; }
        public ulong? Role { get; set; }
        public bool Secret { get; set; }
        public bool ConsumeOnUse { get; set; }
        public string SecretValue { get; set; }
        public DateTime DateAdded { get; set; }
    }
}