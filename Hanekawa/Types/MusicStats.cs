using SharpLink.Stats;

namespace Hanekawa.Types
{
    public class MusicStats
    {
        public CPUStats Cpu { get; set; }
        public FrameStats FrameStats { get; set; }
        public MemoryStats Memory { get; set; }
        public int Players { get; set; }
        public int PlayingPlayers { get; set; }
        public long Uptime { get; set; }
    }
}
