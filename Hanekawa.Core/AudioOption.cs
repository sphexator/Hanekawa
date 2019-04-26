using System.Collections.Generic;
using Discord;
using Victoria.Entities;

namespace Hanekawa.Core
{
    public struct AudioOption
    {
        public bool Shuffle { get; set; }
        public bool Loop { get; set; }
        public bool RepeatTrack { get; set; }
        public MusicMode Mode { get; set; }
        public IUser Summoner { get; set; }
        public LavaTrack VotedTrack { get; set; }
        public HashSet<ulong> Voters { get; set; }
    }
}