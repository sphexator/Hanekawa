namespace Hanekawa.Addons.Database.Tables.Config
{
    public class EventPayout
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int Amount { get; set; } = 100;
    }
}