using System.Collections.Generic;

namespace Hanekawa.Database.Tables.Music
{
    public class Playlist
    {
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public List<string> Songs { get; set; }
    }
}