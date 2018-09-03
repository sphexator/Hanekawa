using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Google.Apis.YouTube.v3;
using Hanekawa.Extensions;
using SharpLink;

namespace Hanekawa.Services.Audio
{
    public class AudioService
    {
        private readonly LavalinkManager _lavalinkManager;
        private readonly YouTubeService _youTubeService;
        private readonly DiscordSocketClient _client;

        private ConcurrentDictionary<ulong, ConcurrentQueue<LavalinkTrack>> MusicQueue { get; }
            = new ConcurrentDictionary<ulong, ConcurrentQueue<LavalinkTrack>>();
        private ConcurrentDictionary<ulong, bool> LoopToggle { get; }
            = new ConcurrentDictionary<ulong, bool>();

        public AudioService(LavalinkManager lavalinkManager, YouTubeService youTubeService, DiscordSocketClient client)
        {
            _lavalinkManager = lavalinkManager;
            _youTubeService = youTubeService;
            _client = client;

            _lavalinkManager.TrackEnd += PlayerEnd;
            _lavalinkManager.TrackStuck += PlayerStuck;
            _lavalinkManager.TrackException += PlayerException;
        }

        private async Task PlayerManager(LavalinkPlayer player, LavalinkTrack track, ulong id)
        {
            if (player.CurrentTrack == null) await player.PlayAsync(track);
            AddToQueue(id, track);
        }

        public async Task<EmbedBuilder> Play(ulong id, IVoiceChannel channel, string query)
        {
            var player = await _lavalinkManager.GetOrCreatePlayer(id, channel);
            var track = await GetSong(query);
            await PlayerManager(player, track, id);
            var author = new EmbedAuthorBuilder
            {
                Name = $"Queued: {track.Title}",
                IconUrl = "https://i.imgur.com/DIi4O65.png",
                Url = track.Url
            };
            var embed = new EmbedBuilder
            {
                Author = author,
                Color = Color.Purple
            };
            return embed;
        }

        public async Task<EmbedBuilder> Start(IGuild guild, IVoiceChannel channel)
        {
            var player = await _lavalinkManager.GetOrCreatePlayer(guild.Id, channel);
            if (player.CurrentTrack == null)
            {
                return new EmbedBuilder().Reply("No song qued");
            }

            try
            {
                await player.PlayAsync(player.CurrentTrack);
                var footer = new EmbedFooterBuilder
                {
                    IconUrl = "https://i.imgur.com/0W6GueZ.png",
                    Text = "Resumed player"
                };
                return new EmbedBuilder
                {
                    Footer = footer,
                    Color = Color.Purple
                };
            }
            catch
            {
                return new EmbedBuilder().Reply(":shrug:");
            }
        }

        public async Task<EmbedBuilder> Pause(IGuild guild, IVoiceChannel channel)
        {
            var player = await _lavalinkManager.GetOrCreatePlayer(guild.Id, channel);
            if (!player.Playing)
            {
                return new EmbedBuilder().Reply("Player isn't playing");
            }

            await player.PauseAsync();
            var footer = new EmbedFooterBuilder
            {
                IconUrl = "https://i.imgur.com/JTAkBLw.png",
                Text = "Paused player"
            };
            var embed = new EmbedBuilder
            {
                Footer = footer,
                Color = Color.Purple
            };
            return embed;
        }

        public async Task Stop(IGuild guild)
        {
            var player = _lavalinkManager.GetPlayer(guild.Id);
            if (player == null) return;
            var queue = MusicQueue.GetOrAdd(guild.Id, new ConcurrentQueue<LavalinkTrack>());
            queue.Clear();
            await player.StopAsync();
        }

        public async Task Destroy(IGuild guild)
        {
            var player = _lavalinkManager.GetPlayer(guild.Id);
            if (player == null) return;
            await player.StopAsync();
            await player.DisconnectAsync();
        }

        public async Task Summon(IGuild guild, IVoiceChannel vc)
        {
            var player = await _lavalinkManager.GetOrCreatePlayer(guild.Id, vc);
            if (!player.Playing) await _lavalinkManager.GetOrCreatePlayer(guild.Id, vc);
            if (player.CurrentTrack == null) await _lavalinkManager.GetOrCreatePlayer(guild.Id, vc);
            if (player.VoiceChannel != vc) return;
            await _lavalinkManager.GetOrCreatePlayer(guild.Id, vc);
        }

        public async Task Reconnect(IGuild guild, IVoiceChannel ch)
        {
            var player = _lavalinkManager.GetPlayer(guild.Id);
            if (player != null)
            {
                await _lavalinkManager.LeaveAsync(guild.Id);
                await _lavalinkManager.GetOrCreatePlayer(guild.Id, ch);
            }
        }

        public async Task SetVolume(IGuild guild, uint volume)
        {
            var player = _lavalinkManager.GetPlayer(guild.Id);
            if (player == null) return;
            await player.SetVolumeAsync(volume);
        }

        public async Task SkipSong(IGuild guild, IVoiceChannel channel)
        {
            var player = await _lavalinkManager.GetOrCreatePlayer(guild.Id, channel);
            await player.StopAsync();
        }

        private async Task<LavalinkTrack> GetSong(string query)
        {
            try
            {
                var track = await _lavalinkManager.GetTrackAsync(query);
                return track;
            }
            catch
            {
                var song = await GetVideoIdByKeywordsAsync(query);
                var track = await _lavalinkManager.GetTrackAsync($"https://www.youtube.com/watch?v={song.FirstOrDefault()}");
                return track;
            }
        }

        public EmbedBuilder ToggleLoop(IGuild guild)
        {
            LoopToggle.AddOrUpdate(guild.Id, true, (key, old) =>
            {
                old = !old;
                return old;
            });
            var value = LoopToggle.GetOrAdd(guild.Id, false);
            var toggle = value ? "disabled" : "enabled";
            return new EmbedBuilder().Reply($"Loop has been {toggle}");
        }

        //Queue
        public LavalinkTrack GetCurrentTrack(IGuild guild)
        {
            return _lavalinkManager.GetPlayer(guild.Id).CurrentTrack;
        }

        public LavalinkPlayer GetCurrentPlayer(IGuild guild)
        {
            return _lavalinkManager.GetPlayer(guild.Id);
        }

        private void AddToQueue(ulong id, LavalinkTrack track)
        {
            var queue = MusicQueue.GetOrAdd(id, new ConcurrentQueue<LavalinkTrack>());
            queue.Enqueue(track);
        }

        public EmbedBuilder GetCurrentSong(IGuild guild)
        {
            var player = _lavalinkManager.GetPlayer(guild.Id);
            if (player.CurrentTrack == null) return null;
            var song = player.CurrentTrack;
            var author = new EmbedAuthorBuilder
            {
                Name = $"Song: {song.Title}",
                IconUrl = "https://i.imgur.com/DIi4O65.png",
                Url = song.Url
            };
            var embed = new EmbedBuilder
            {
                Author = author,
                Color = Color.Purple
            };
            return embed;
        }

        public IEnumerable<LavalinkTrack> GetQueue(ulong guildid)
        {
            return MusicQueue.GetOrAdd(guildid, new ConcurrentQueue<LavalinkTrack>());
        }

        public void ClearQueue(ulong guildid)
        {
            var queue = MusicQueue.GetOrAdd(guildid, new ConcurrentQueue<LavalinkTrack>());
            queue.Clear();
        }

        //playlist
        public async Task<int> AddPlaylistToQueue(string playlistString, IGuildUser user, IVoiceChannel ch)
        {
            var queue = MusicQueue.GetOrAdd(user.GuildId, new ConcurrentQueue<LavalinkTrack>());
            var request = (await GetPlaylistIdsByKeywordsAsync(playlistString).ConfigureAwait(false)).FirstOrDefault();
            var songs = await GetPlaylistTracksAsync(request, 5000).ConfigureAwait(false);
            var songList = songs.ToList();
            var player = await _lavalinkManager.GetOrCreatePlayer(user.GuildId, ch);
            foreach (var x in songList)
            {
                try
                {
                    if (songList.First() == x && player.CurrentTrack == null)
                    {
                        await player.PlayAsync(await _lavalinkManager.GetTrackAsync($"https://www.youtube.com/watch?v={x}")
                            .ConfigureAwait(false));
                    }
                    else
                    {
                        queue.Enqueue(await _lavalinkManager.GetTrackAsync($"https://www.youtube.com/watch?v={x}")
                            .ConfigureAwait(false));
                    }
                }
                catch
                {
                    /*ignore */
                }
            }

            return songList.Count;
        }

        private async Task<IEnumerable<string>> GetPlaylistTracksAsync(string playlistId, int count = 50)
        {
            await Task.Yield();
            if (string.IsNullOrWhiteSpace(playlistId))
                throw new ArgumentNullException(nameof(playlistId));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            string nextPageToken = null;

            var toReturn = new List<string>(count);

            do
            {
                var toGet = count > 50 ? 50 : count;
                count -= toGet;

                var query = _youTubeService.PlaylistItems.List("contentDetails");
                query.MaxResults = toGet;
                query.PlaylistId = playlistId;
                query.PageToken = nextPageToken;

                var data = await query.ExecuteAsync();

                toReturn.AddRange(data.Items.Select(i => i.ContentDetails.VideoId));
                nextPageToken = data.NextPageToken;
            } while (count > 0 && !string.IsNullOrWhiteSpace(nextPageToken));

            return toReturn;
        }

        private static readonly Regex PlRegex = new Regex("(?:youtu\\.be\\/|list=)(?<id>[\\da-zA-Z\\-_]*)",
            RegexOptions.Compiled);

        private async Task<IEnumerable<string>> GetPlaylistIdsByKeywordsAsync(string keywords, int count = 1)
        {
            await Task.Yield();
            if (string.IsNullOrWhiteSpace(keywords))
                throw new ArgumentNullException(nameof(keywords));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var match = PlRegex.Match(keywords);
            if (match.Length > 1)
            {
                return new[] { match.Groups["id"].Value };
            }

            var query = _youTubeService.Search.List("snippet");
            query.MaxResults = count;
            query.Type = "playlist";
            query.Q = keywords;

            return (await query.ExecuteAsync()).Items.Select(i => i.Id.PlaylistId);
        }

        private async Task<IEnumerable<string>> GetVideoIdByKeywordsAsync(string keywords, int count = 1)
        {
            await Task.Yield();
            if (string.IsNullOrWhiteSpace(keywords))
                throw new ArgumentNullException(nameof(keywords));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var match = PlRegex.Match(keywords);
            if (match.Length > 1)
            {
                return new[] { match.Groups["id"].Value };
            }

            var query = _youTubeService.Search.List("snippet");
            query.MaxResults = count;
            query.Type = "video";
            query.Q = keywords;

            return (await query.ExecuteAsync()).Items.Select(i => i.Id.VideoId);
        }

        // Event handlers
        private async Task PlayerException(LavalinkPlayer player, LavalinkTrack track, string arg3)
        {
            Console.WriteLine(arg3);
            var loop = LoopToggle.GetOrAdd(player.VoiceChannel.GuildId, false);
            if (loop) await LoopSong(player, track);
            else await ContinueSong(player, track);

        }

        private async Task PlayerStuck(LavalinkPlayer player, LavalinkTrack track, long arg3)
        {
            Console.WriteLine(arg3);
            var loop = LoopToggle.GetOrAdd(player.VoiceChannel.GuildId, false);
            if (loop) await LoopSong(player, track);
            else await ContinueSong(player, track);
        }

        private async Task PlayerEnd(LavalinkPlayer player, LavalinkTrack track, string arg3)
        {
            Console.WriteLine(arg3);
            var loop = LoopToggle.GetOrAdd(player.VoiceChannel.GuildId, false);
            if (loop) await LoopSong(player, track);
            else await ContinueSong(player, track);
        }

        private async Task LoopSong(LavalinkPlayer player, LavalinkTrack track)
        {
            var queue = MusicQueue.GetOrAdd(player.VoiceChannel.GuildId, new ConcurrentQueue<LavalinkTrack>());
            queue.TryPeek(out var next);
            if (queue.Count < 1) return;
            if (queue.Count == 1)
            {
                queue.TryDequeue(out var song);
                queue.Enqueue(song);
                await player.PlayAsync(next);
            }
            else if (track == next)
            {
                queue.TryDequeue(out var song);
                queue.Enqueue(next);
                MusicQueue.GetOrAdd(player.VoiceChannel.GuildId, new ConcurrentQueue<LavalinkTrack>()).TryDequeue(out var getNextSong);
                queue.Enqueue(getNextSong);
                await player.PlayAsync(getNextSong);
            }
            else
            {
                queue.TryDequeue(out var song);
                queue.Enqueue(song);
                await player.PlayAsync(song);
            }
        }

        private async Task ContinueSong(LavalinkPlayer player, LavalinkTrack track)
        {
            var queue = MusicQueue.GetOrAdd(player.VoiceChannel.GuildId, new ConcurrentQueue<LavalinkTrack>());
            queue.TryPeek(out var next);
            if (queue.Count < 1) return;
            if (queue.Count == 1)
            {
                queue.Clear();
            }
            else if (track == next)
            {
                queue.TryDequeue(out var result);
                queue.TryPeek(out var nextSong);
                await player.PlayAsync(nextSong);
            }
            else
            {
                queue.TryDequeue(out var song);
                await player.PlayAsync(song);
            }
        }
    }
}
