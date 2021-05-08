using System;
using Disqord;

namespace Hanekawa.Database.Tables.Config
{
    public class WelcomeBanner
    {
        public Snowflake GuildId { get; set; }
        public int Id { get; set; }
        public string Url { get; set; }
        public Snowflake Uploader { get; set; }
        public bool IsNsfw { get; set; } = false;
        // Avatar configurations
        public int AvatarSize { get; set; } = 60;
        public int AviPlaceX { get; set; } = 10;
        public int AviPlaceY { get; set; } = 10;

        // Name Placement
        public int TextSize { get; set; } = 33;
        public int TextPlaceX { get; set; } = 245;
        public int TextPlaceY { get; set; } = 40;

        public DateTimeOffset UploadTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    }
}