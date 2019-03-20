using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Victoria;
using Victoria.Entities;

namespace Hanekawa.Extensions
{
    public static class AudioExtension
    {
        public static async Task<LavaPlayer> GetOrCreatePlayerAsync(this LavaNode node, ulong guildId,
            IVoiceChannel vc, IMessageChannel txC)
        {
            var player = node.GetPlayer(guildId);
            if (player == null) return await node.ConnectAsync(vc, txC);
            if (player.VoiceChannel == null) return await node.ConnectAsync(vc, txC);
            return player;
        }

        public static async Task<LavaTrack> GetSongAsync(this LavaNode manager, string url)
        {
            LavaTrack result;
            var check = Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
            if (check) result = await TryGetSongFromUrl(manager, url);
            else result = await TryGetSongFromQuotesAsync(manager, url);
            return result ?? await TryGetSongFromQuotesAsync(manager, url);
        }

        private static async Task<LavaTrack> TryGetSongFromUrl(this LavaNode manager, string url)
        {
            var track = (await manager.GetTracksAsync(url)).Tracks.FirstOrDefault();
            return track ?? null;
        }

        private static async Task<LavaTrack> TryGetSongFromQuotesAsync(this LavaNode manager, string url)
        {
            var tracks = (await manager.SearchYouTubeAsync(url)).Tracks.FirstOrDefault();
            return tracks ?? null;
        }
    }
}