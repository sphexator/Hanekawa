using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using SharpLink;

namespace Jibril.Extensions
{
    public static class AudioExtension
    {
        public static async Task<LavalinkPlayer> GetOrCreatePlayer(this LavalinkManager manager, ulong id, IVoiceChannel channel)
        {
            var player = manager.GetPlayer(id);
            if (player != null) return player;
            var newPlayer = await manager.JoinAsync(channel);
            return newPlayer;
        }
    }
}
