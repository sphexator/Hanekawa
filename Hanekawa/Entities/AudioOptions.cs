using Discord;
using System.Collections.Generic;
using Victoria.Entities;

namespace Hanekawa.Entities
{
    public struct AudioOptions
    {
        public bool Shuffle { get; set; }
        public bool Loop { get; set; }
        public bool RepeatTrack { get; set; }
        public IUser Summoner { get; set; }
        public LavaTrack VotedTrack { get; set; }
        public HashSet<ulong> Voters { get; set; }
    }
}