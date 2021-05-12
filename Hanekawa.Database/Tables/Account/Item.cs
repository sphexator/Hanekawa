using System;
using System.Collections.Generic;
using Hanekawa.Database.Entities.Items;
using Newtonsoft.Json;

namespace Hanekawa.Database.Tables.Account
{
    public class Item
    {
        public Guid Id { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public IItem ItemJson { get; set; }
        public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;

        public List<Inventory> Users = null;
    }
}