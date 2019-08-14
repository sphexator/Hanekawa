namespace Hanekawa.Database.Tables.Account
{
    public class Highlight
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public string[] Highlights { get; set; }
    }
}