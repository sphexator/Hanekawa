﻿namespace Hanekawa.Addons.Database.Tables.Stores
{
    public class Shop
    {
        public ulong GuildId { get; set; }
        public int ItemId { get; set; }
        public int Price { get; set; }
        public bool SpecialCredit { get; set; }
    }
}