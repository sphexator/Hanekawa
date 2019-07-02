using System.Collections.Concurrent;
using Hanekawa.Shared;

namespace Hanekawa.Bot.Services.Music
{
    public partial class MusicService
    {
        private readonly ConcurrentDictionary<ulong, AudioOption> _audioOptions = new ConcurrentDictionary<ulong, AudioOption>();

    }
}
