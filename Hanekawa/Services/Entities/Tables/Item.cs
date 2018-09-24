namespace Hanekawa.Services.Entities.Tables
{
    public class Item
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public bool Unique { get; set; }
        public bool StackAble { get; set; }
    }
}
