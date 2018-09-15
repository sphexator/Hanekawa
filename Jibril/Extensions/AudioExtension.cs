using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using SharpLink;

namespace Hanekawa.Extensions
{
    public static class AudioExtension
    {
        private static readonly Regex SpotifyRegex = new Regex(@"/(https?:\/\/open.spotify.com\/(track|user|artist|album)\/[a-zA-Z0-9]+(\/playlist\/[a-zA-Z0-9]+|)|spotify:(track|user|artist|album):[a-zA-Z0-9]+(:playlist:[a-zA-Z0-9]+|))/");
        private static readonly Regex PlayListRegex = new Regex("(?:youtu\\.be\\/|list=)(?<id>[\\da-zA-Z\\-_]*)",
            RegexOptions.Compiled);

        public static async Task<LavalinkPlayer> GetOrCreatePlayer(this LavalinkManager manager, ulong id,
            IVoiceChannel channel)
        {
            var player = manager.GetPlayer(id);
            if (player != null) return player;
            var newPlayer = await manager.JoinAsync(channel);
            return newPlayer;
        }

        public static async Task<LavalinkTrack> TryParseSong(this LavalinkManager manager, string url)
        {
            //if(SpotifyRegex.IsMatch(url))
            //if(PlayListRegex.IsMatch(url))
            return (await GetSong(manager, url)).Tracks.FirstOrDefault();
        }

        private static async Task<LoadTracksResponse> GetSong(this LavalinkManager manager, string url)
        {
            var track = await TryGetSongFromUrl(manager, url);
            if (track.Tracks.Count != 0) return track;
            return await TryGetSongFromQuotesAsync(manager, url);
        }

        private static async Task<LoadTracksResponse> TryGetSongFromUrl(this LavalinkManager manager, string url)
        {
            var track = await manager.GetTracksAsync(url);
            return track ?? null;
        }

        private static async Task<LoadTracksResponse> TryGetSongFromQuotesAsync(this LavalinkManager manager, string url)
        {
            var tracks = await manager.GetTracksAsync($"ytsearch:{url}");
            return tracks ?? null;
        }
    }
}
