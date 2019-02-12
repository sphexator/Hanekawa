using System;

namespace Hanekawa.Addons.Database.Tables.Config
{
    public class WelcomeBanner
    {
        public ulong GuildId { get; set; }
        public int Id { get; set; }
        public string Url { get; set; }
        public ulong Uploader { get; set; }
        public DateTimeOffset UploadTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    }
}