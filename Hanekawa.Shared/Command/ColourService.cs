using System.Collections.Concurrent;
using System.Runtime.InteropServices.ComTypes;
using Discord;
using Hanekawa.Shared.Interfaces;

namespace Hanekawa.Shared.Command
{
    public class ColourService : INService
    {
        private readonly ConcurrentDictionary<ulong, Color> _colours = new ConcurrentDictionary<ulong, Color>();

        public Color Get(ulong guildId)
            => _colours.TryGetValue(guildId, out var color) ? color : Color.Purple;

        public void AddOrUpdate(ulong guildId, Color color)
            => _colours.AddOrUpdate(guildId, color, (k, v) => color);

        public bool TryRemove(ulong guildId) => _colours.TryRemove(guildId, out _);
        
    }
}