using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Drop
{
    public partial class DropService
    {
        private readonly MemoryCache _guildCooldown = new MemoryCache(new MemoryCacheOptions());
        
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _lootChannels
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();
        
        private readonly ConcurrentDictionary<ulong, MemoryCache> _normalLoot =
            new ConcurrentDictionary<ulong, MemoryCache>();
        
        private readonly ConcurrentDictionary<ulong, MemoryCache> _spawnedLoot =
            new ConcurrentDictionary<ulong, MemoryCache>();
        
        private readonly ConcurrentDictionary<ulong, MemoryCache> _userCooldown =
            new ConcurrentDictionary<ulong, MemoryCache>();

        private bool IsDropMessage()
        {
            
        }

        private bool IsDropChannel()
        {
            
        }

        private bool OnUserCooldown()
        {
            
        }

        private bool OnGuildCooldown()
        {
            
        }
        
        
    }
}