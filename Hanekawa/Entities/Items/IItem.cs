namespace Hanekawa.Entities.Items
{
    public interface IItem
    {
        string Name { get; set; }
        public int? Sell { get; set; }
    }
}