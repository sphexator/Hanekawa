using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria;
using Victoria.Objects;

namespace Hanekawa.Extensions
{
    public static class AudioExtension
    {
        public static async Task<LavaPlayer> GetOrCreatePlayerAsync(this LavaNode node, ulong guildId, IVoiceChannel vc, IMessageChannel txC)
        {
            var player = node.GetPlayer(guildId);
            if (player == null)
            {
                player =  await node.JoinAsync(vc, txC);
                player.Queue.TryAdd(guildId, new LinkedList<LavaTrack>());
                return player;
            }
            player.Queue.TryAdd(guildId, new LinkedList<LavaTrack>());
            if (!player.IsConnected) return await node.JoinAsync(vc, txC);
            return player;
        }

        public static async Task<LavaResult> GetSongAsync(this LavaNode manager, string url)
        {
            var check = Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
            if (check) return await TryGetSongFromUrl(manager, new Uri(url));
            return await TryGetSongFromQuotesAsync(manager, url);
        }

        private static async Task<LavaResult> TryGetSongFromUrl(this LavaNode manager, Uri url)
        {
            var track = await manager.GetTracksAsync(url);
            return track ?? null;
        }

        private static async Task<LavaResult> TryGetSongFromQuotesAsync(this LavaNode manager, string url)
        {
            var tracks = await manager.SearchYouTubeAsync(url);
            return tracks ?? null;
        }
    }
}
