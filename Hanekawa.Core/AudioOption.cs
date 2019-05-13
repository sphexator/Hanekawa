namespace Hanekawa.Core
{
    public class AudioOption
    {
        public bool Shuffle { get; set; } = false;
        public bool Loop { get; set; } = false;
        public bool RepeatTrack { get; set; } = false;
        public MusicMode Mode { get; set; } = MusicMode.Music;
    }
}