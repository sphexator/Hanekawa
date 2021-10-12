using System;
using System.ComponentModel.DataAnnotations;
using Disqord;

namespace Hanekawa.Database.Tables.Config
{
    public class LevelExpEvent
    {
        public Guid Id { get; set; }
        public Snowflake GuildId { get; set; }
        
        public double Multiplier { get; set; }
        
        [Required]
        public DateTimeOffset Start { get; set; } = DateTimeOffset.UtcNow;
        [Required]
        public DateTimeOffset End { get; set; }
    }
}