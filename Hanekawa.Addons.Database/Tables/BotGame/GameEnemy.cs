namespace Hanekawa.Addons.Database.Tables.BotGame
{
    public class GameEnemy
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public bool Elite { get; set; }
        public bool Rare { get; set; }

        public string ImageUrl { get; set; }

        public int Health { get; set; }
        public int Damage { get; set; }

        public int ClassId { get; set; }

        public int ExpGain { get; set; }
        public int CreditGain { get; set; }
    }
}