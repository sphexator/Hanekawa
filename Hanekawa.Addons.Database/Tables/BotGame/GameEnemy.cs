namespace Hanekawa.Addons.Database.Tables.BotGame
{
    public class GameEnemy
    {
        public int Id { get; set; }
        public string Name { get; set; } = "test";

        public bool Elite { get; set; } = false;
        public bool Rare { get; set; } = false;

        public string ImageUrl { get; set; } = null;

        public int Health { get; set; } = 100;
        public int Damage { get; set; } = 10;

        public int ClassId { get; set; } = 1;

        public int ExpGain { get; set; } = 10;
        public int CreditGain { get; set; } = 10;
    }
}