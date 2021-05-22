using System;
using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.Quote
{
    public class Quote
    {
        /// <summary>
        /// Guild ID
        /// </summary>
        public Snowflake GuildId { get; set; }
        /// <summary>
        /// Key for quote
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Quoted message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Date added
        /// </summary>
        public DateTimeOffset Added { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// Creator of quote
        /// </summary>
        public Snowflake Creator { get; set; }
        /// <summary>
        /// Trigger to auto-post quote
        /// </summary>
        public List<string> Triggers { get; set; } = null;
        /// <summary>
        /// Level requirement for auto-trigger
        /// </summary>
        public int LevelCap { get; set; } = 0;
    }
}