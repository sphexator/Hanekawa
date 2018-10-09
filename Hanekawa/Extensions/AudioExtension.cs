using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;
using SharpLink;

namespace Hanekawa.Extensions
{
    public static class AudioExtension
    {
        public static async Task<LavalinkPlayer> GetOrCreatePlayerAsync(this LavalinkManager node, ulong guildId, IVoiceChannel vc, IMessageChannel txC)
        {
            var player = node.GetPlayer(guildId);
            if (player == null) return await node.JoinAsync(vc);
            if (player.VoiceChannel == null) return await node.JoinAsync(vc);
            return player;
        }

        public static async Task<LavalinkTrack> GetSongAsync(this LavalinkManager manager, string url)
        {
            LavalinkTrack result;
            var check = Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
            if (check) result = await TryGetSongFromUrl(manager, url);
            else result = await TryGetSongFromQuotesAsync(manager, url);
            return result ?? await TryGetSongFromQuotesAsync(manager, url);
        }

        private static async Task<LavalinkTrack> TryGetSongFromUrl(this LavalinkManager manager, string url)
        {
            var track = (await manager.GetTracksAsync(url)).Tracks.FirstOrDefault();
            return track ?? null;
        }

        private static async Task<LavalinkTrack> TryGetSongFromQuotesAsync(this LavalinkManager manager, string url)
        {
            var tracks = (await manager.GetTracksAsync($"ytsearch:{url}")).Tracks.FirstOrDefault();
            return tracks ?? null;
        }
    }
}
