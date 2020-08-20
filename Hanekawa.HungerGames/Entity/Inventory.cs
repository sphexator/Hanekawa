namespace Hanekawa.HungerGames.Entity
{
    public class Inventory
    {
        public int Id { get; set; }
        public Participant User { get; set; }

        public int Count { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }
    }
}