namespace Hanekawa.Addons.Database.Tables.Account
{
    public class Item
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public bool Unique { get; set; }
        public bool StackAble { get; set; }
    }
}
