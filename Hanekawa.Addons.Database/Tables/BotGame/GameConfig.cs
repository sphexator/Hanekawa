namespace Hanekawa.Addons.Database.Tables.BotGame
{
    public class GameConfig
    {
        public int Id { get; set; }
        public int DefaultHealth { get; set; } = 10;
        public int DefaultDamage { get; set; } = 1;
    }
}