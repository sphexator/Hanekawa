using Disqord;

namespace Hanekawa.Database.Entities.Items
{
    public class RoleItem : IItem
    {
        public string Name { get; set; }
        public int? Sell { get; set; }
        
        public Snowflake RoleId { get; set; }
    }
}