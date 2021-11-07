using System;
using System.ComponentModel.DataAnnotations;

namespace Hanekawa.Entities.Config.Level
{
    public class LevelExpEvent
    {
        public Guid Id { get; set; }
        public ulong GuildId { get; set; }
        
        public double Multiplier { get; set; }
        
        [Required]
        public DateTimeOffset Start { get; set; } = DateTimeOffset.UtcNow;
        [Required]
        public DateTimeOffset End { get; set; }
    }
}