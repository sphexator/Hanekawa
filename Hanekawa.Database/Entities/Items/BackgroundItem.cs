namespace Hanekawa.Database.Entities.Items
{
    public class BackgroundItem : IItem
    {
        public string Name { get; set; }
        public int? Sell { get; set; }

        public string Url { get; set; }
    }
}