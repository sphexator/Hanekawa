using System;

namespace Hanekawa.Addons.Database.Tables.Audio
{
    public class Playlist
    {
        public string Id { get; set; }
        public ulong GuildId { get; set; }
        public int Streams { get; set; }
        public bool IsPrivate { get; set; } = true;
        public ulong OwnerId { get; set; }
        public TimeSpan Playtime { get; set; } = TimeSpan.Zero;
    }
}
