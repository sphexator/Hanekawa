namespace Hanekawa.Entities.Items
{
    public class RoleItem : IItem
    {
        public string Name { get; set; }
        public int? Sell { get; set; }
        
        public ulong RoleId { get; set; }
    }
}