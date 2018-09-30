using System;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.Audio;
using Humanizer;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Victoria;

namespace Hanekawa.Modules.Audio.Service
{
    public class PlaylistService
    {
        private readonly LavaNode LavaNode;

        public Lavalink Lavalink { get; }

        public PlaylistService(Lavalink lavalink)
        {
            LavaNode = lavalink.SingleNode;
            Lavalink = lavalink;
        }

        public async Task<string> TryCreate(Playlist playlist)
        {
            if (playlist.Id.Length > 12)
            {
                return "Playlist name must be less than 12 characters.";
            }

            using (var db = new DbService())
            {
                playlist.Id = playlist.Id.Humanize();
                if (await db.Playlists.FindAsync(playlist.GuildId, playlist.Id) != null)
                {
                    return $"{playlist.Id} name is taken.";
                }

                await db.Playlists.AddAsync(playlist);
                db.SaveChanges();
                return $"{playlist.Id} has been created.";
            }
        }

        public bool TryModify(Playlist playlist, ulong userId, out string message)
        {
            message = "Not yet implemented";
            return false;
        }

        public async Task<string> TryDelete(string playlistId, ulong userId, ulong guildId)
        {
            playlistId.Humanize();

            using (var db = new DbService())
            {
                var load = await db.Playlists.FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == playlistId);
                if (load == null)
                {
                    return $"{playlistId} doesn't exist.";
                }

                if (load.OwnerId != userId)
                {
                    return "You can't delete someone elses playlist.";
                }

                db.Playlists.Remove(load);
                return $"{playlistId} has been deleted.";
            }
        }

        public bool TryGet(string playlistId, ulong guildId, out Playlist playlist)
        {
            using (var db = new DbService())
            {
                var load = db.Playlists.Find(guildId, playlistId);
                if (load == null)
                {
                    playlist = null;
                    return false;
                }

                if (load.IsPrivate)
                {
                    playlist = load;
                    return false;
                }

                playlist = load;
                return true;
            }
        }

        public async Task<string> TryPlay(string playlistId, ulong guildId, ulong userId, IMessageChannel channel)
        {
            using (var db = new DbService())
            {
                var load = await db.Playlists.FindAsync(guildId, playlistId);
                if (load == null)
                {
                    return $"{playlistId} doesn't exist.";
                }

                if (load.IsPrivate && load.OwnerId != userId)
                {
                    return $"{playlistId} doesn't exist.";
                }

                if (load.Tracks.Count == 0)
                {
                    return "Playlist is empty.";
                }

                var player = LavaNode.GetPlayer(guildId);
                if (player == null)
                {
                    return "Must be connected to a voice channel.";
                }

                foreach (var trackId in load.Tracks)
                {
                    var track = Util.DecodeTrack(trackId);
                    player.Play(track);
                }

                load.Streams++;
                await db.SaveChangesAsync();
                return $"Enqueued {load.Tracks.Count} tracks.";
            }
        }
    }
}
