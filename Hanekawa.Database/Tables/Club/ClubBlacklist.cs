using System;
using Disqord;
namespace Hanekawa.Database.Tables.Club
{
    public class ClubBlacklist
    {
        public Snowflake GuildId { get; set; }
        public Snowflake User { get; set; }
        public Snowflake Issuer { get; set; }
        public string Reason { get; set; } = "No reason provided";
        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
        
        public Guid ClubId { get; set; }
        public Club Club { get; set; }
    }
}