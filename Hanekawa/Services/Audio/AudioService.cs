using Discord;
using Hanekawa.Extensions;
using Hanekawa.Types;
using SharpLink;
using SharpLink.Stats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;

namespace Hanekawa.Services.Audio
{
    public class AudioService
    {
        private readonly LavalinkManager _lavalinkManager;

        private ConcurrentDictionary<ulong, ConcurrentQueue<LavalinkTrack>> MusicQueue { get; }
            = new ConcurrentDictionary<ulong, ConcurrentQueue<LavalinkTrack>>();
        private ConcurrentDictionary<ulong, bool> LoopToggle { get; }
            = new ConcurrentDictionary<ulong, bool>();
        private MusicStats _stats = new MusicStats();

        public AudioService(LavalinkManager lavalinkManager)
        {
            _lavalinkManager = lavalinkManager;

            _lavalinkManager.TrackEnd += PlayerEndAsync;
            _lavalinkManager.TrackStuck += PlayerStuckAsync;
            _lavalinkManager.TrackException += PlayerExceptionAsync;
            _lavalinkManager.Stats += PlayerStatsAsync;
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
                Color = Color.DarkPurple
            };
            return embed;
        }

        public async Task<EmbedBuilder> Start(IGuild guild, IVoiceChannel channel)
        {
            var player = await _lavalinkManager.GetOrCreatePlayer(guild.Id, channel);
            if (player.CurrentTrack == null)
            {
                return new EmbedBuilder().Reply("No song queued");
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
                    Color = Color.DarkPurple
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
                Color = Color.DarkPurple
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

        private async Task<LavalinkTrack> GetSong(string query) => await _lavalinkManager.TryParseSong(query);

        public EmbedBuilder ToggleLoop(IGuild guild)
        {
            LoopToggle.AddOrUpdate(guild.Id, true, (key, old) =>
            {
                old = !old;
                return old;
            });
            var value = LoopToggle.GetOrAdd(guild.Id, false);
            var toggle = value ? "enabled" : "disabled";
            return new EmbedBuilder().Reply($"Loop has been {toggle}");
        }

        public EmbedBuilder MusicStats()
        {
            var embed = new EmbedBuilder
            {
                Color = Color.Purple,
            };

            embed.AddField("CPU", $"{_stats.Cpu.LavalinkLoad}%", true);
            embed.AddField("Players", $"{_stats.Players}", true);
            embed.AddField("PlayingPlayers", $"{_stats.PlayingPlayers}", true);
            try
            {
                embed.AddField("FrameStats", $"**Deficit:** {_stats.FrameStats.Deficit}\n" +
                                             $"**Nulled:** {_stats.FrameStats.Nulled}\n" +
                                             $"**Sent:** {_stats.FrameStats.Sent}", true);
            }
            catch {/* IGNORE */}
            embed.AddField("Memory", $"**Allocated:** {_stats.Memory.Allocated.SizeSuffix()}\n" +
                                     $"**Free:** {_stats.Memory.Free.SizeSuffix()}\n" +
                                     $"**Reservable:** {_stats.Memory.Reservable.SizeSuffix()}\n" +
                                     $"**Used:** {_stats.Memory.Used.SizeSuffix()}\n", true);
            embed.AddField("Uptime", $"{TimeSpan.FromMilliseconds(_stats.Uptime).Humanize()}");
            return embed;
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
                Color = Color.DarkPurple
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
        public async Task<int> AddPlaylistToQueueAsync(string playlistString, IGuildUser user, IVoiceChannel ch)
        {
            var queue = MusicQueue.GetOrAdd(user.GuildId, new ConcurrentQueue<LavalinkTrack>());
            var request = await _lavalinkManager.GetTracksAsync(playlistString);
            var player = await _lavalinkManager.GetOrCreatePlayer(user.GuildId, ch);
            foreach (var x in request.Tracks)
            {
                try
                {
                    if (request.Tracks.First() == x && player.CurrentTrack == null)
                    {
                        await player.PlayAsync((await _lavalinkManager.GetTracksAsync(x.Url)).Tracks.FirstOrDefault());
                    }
                    else
                    {
                        queue.Enqueue((await _lavalinkManager.GetTracksAsync($"https://www.youtube.com/watch?v={x}")).Tracks.FirstOrDefault());
                    }
                }
                catch
                {
                    /*ignore */
                }
            }

            return request.Tracks.Count;
        }

        // Event handlers
        private async Task PlayerExceptionAsync(LavalinkPlayer player, LavalinkTrack track, string arg3)
        {
            Console.WriteLine(arg3);
            var loop = LoopToggle.GetOrAdd(player.VoiceChannel.GuildId, false);
            if (loop) await LoopQueueAsync(player, track);
            else await ContinueQueueAsync(player, track);

        }

        private async Task PlayerStuckAsync(LavalinkPlayer player, LavalinkTrack track, long arg3)
        {
            Console.WriteLine(arg3);
            var loop = LoopToggle.GetOrAdd(player.VoiceChannel.GuildId, false);
            if (loop) await LoopQueueAsync(player, track);
            else await ContinueQueueAsync(player, track);
        }

        private async Task PlayerEndAsync(LavalinkPlayer player, LavalinkTrack track, string arg3)
        {
            Console.WriteLine(arg3);
            var loop = LoopToggle.GetOrAdd(player.VoiceChannel.GuildId, false);
            if (loop) await LoopQueueAsync(player, track);
            else await ContinueQueueAsync(player, track);
        }

        private Task PlayerStatsAsync(LavalinkStats stats)
        {
            var result = new MusicStats
            {
                Cpu = stats.CPU,
                FrameStats = stats.FrameStats,
                Memory = stats.Memory,
                Players = stats.Players,
                PlayingPlayers = stats.PlayingPlayers,
                Uptime = stats.Uptime
            };
            _stats = result;
            Console.WriteLine("Updated stats");
            return Task.CompletedTask;
        }

        private async Task LoopQueueAsync(LavalinkPlayer player, LavalinkTrack track)
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

        private async Task ContinueQueueAsync(LavalinkPlayer player, LavalinkTrack track)
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
