using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Extensions;
using SharpLink;

namespace Hanekawa.Modules.Audio.Service
{
    public class AudioService
    {
        private const int MaxTries = 100;
        private readonly DiscordSocketClient _client;
        private readonly LavalinkManager _lava;
        private readonly ConcurrentDictionary<ulong, int> _queNumber;
        private readonly ConcurrentDictionary<ulong, List<LavalinkTrack>> _queue;
        private readonly ConcurrentDictionary<ulong, bool> _repeat;
        private readonly ConcurrentDictionary<ulong, ITextChannel> _textChannel;
        private readonly ConcurrentDictionary<ulong, (LavalinkTrack Track, List<ulong> Votes)> _voteSkip;

        public AudioService(DiscordSocketClient client, LavalinkManager lava)
        {
            _client = client;
            _lava = lava;

            _client.Ready += OnReady;

            _lava.TrackEnd += OnFinished;
            _lava.TrackException += OnException;
            _lava.TrackStuck += OnStuck;

            _voteSkip = new ConcurrentDictionary<ulong, (LavalinkTrack Track, List<ulong> Votes)>();
            _repeat = new ConcurrentDictionary<ulong, bool>();
            _queue = new ConcurrentDictionary<ulong, List<LavalinkTrack>>();
            _textChannel = new ConcurrentDictionary<ulong, ITextChannel>();
            _queNumber = new ConcurrentDictionary<ulong, int>();
        }

        private async Task OnReady()
        {
            await _lava.StartAsync();
        }

        public async Task<EmbedBuilder> PlayAsync(IGuildUser user, string query, IVoiceState state,
            ITextChannel channel)
        {
            var player = await _lava.GetOrCreatePlayerAsync(user.GuildId, state.VoiceChannel, channel);
            var track = await _lava.GetSongAsync(query);
            if (track == null)
                return new EmbedBuilder
                {
                    Color = Color.Purple,
                    Description = $"{user.Mention}, Couldn't play song"
                };
            return await QueueSongAsync(player, track, channel);
        }

        public async Task<EmbedBuilder> StopAsync(ulong guildId)
        {
            try
            {
                await _lava.LeaveAsync(guildId);
                return new EmbedBuilder().Reply("Disconnected!", Color.Green.RawValue);
            }
            catch
            {
                return new EmbedBuilder().Reply("Can't leave when I'm not connected??", Color.Red.RawValue);
            }
        }

        public async Task<EmbedBuilder> PauseAsync(ulong guildId)
        {
            var player = _lava.GetPlayer(guildId);
            try
            {
                await player.PauseAsync();
                return new EmbedBuilder().Reply($"**Paused:** {player.CurrentTrack.Title}");
            }
            catch
            {
                return new EmbedBuilder().Reply("Not playing anything currently.");
            }
        }

        public async Task<EmbedBuilder> ResumeAsync(ulong guildId)
        {
            var player = _lava.GetPlayer(guildId);
            try
            {
                await player.ResumeAsync();
                return new EmbedBuilder().Reply($"**Resumed:** {player.CurrentTrack.Title}", Color.Green.RawValue);
            }
            catch
            {
                return new EmbedBuilder().Reply("Not playing anything currently.", Color.Red.RawValue);
            }
        }

        public EmbedBuilder DisplayQueue(ulong guildId)
        {
            var player = _lava.GetPlayer(guildId);
            var embed = new EmbedBuilder {Color = Color.Purple};
            try
            {
                if (player.Playing && player.CurrentTrack != null)
                {
                    embed.Title = player.CurrentTrack.Title;
                    embed.Url = player.CurrentTrack.Url;
                }

                string queue = null;
                var limit = 10;
                var chQueue = _queue.TryGetValue(guildId, out var queueList);
                if (queueList != null && queueList.Count < limit) limit = queueList.Count;
                if (chQueue && queueList != null && queueList.Count > 0)
                {
                    var tries = 0;
                    foreach (var x in queueList)
                    {
                        if (tries >= limit) continue;
                        if (player.CurrentTrack != null && player.CurrentTrack.Title == x.Title)
                            queue += $"=> {x.Title}\n";
                        else queue += $"{x.Title}\n";
                        tries++;
                    }
                }
                else
                {
                    queue = "Queue is empty";
                }

                embed.Description = queue;
                return embed;
            }
            catch
            {
                return new EmbedBuilder().Reply("Queue is empty.");
            }
        }

        public async Task<EmbedBuilder> ClearQueueAsync(ulong guildId)
        {
            var player = _lava.GetPlayer(guildId);
            if (player == null) return new EmbedBuilder().Reply("Not playing anything currently.", Color.Red.RawValue);
            if (player.CurrentTrack != null) await player.StopAsync();
            _queue.TryGetValue(guildId, out var queue);
            if (queue == null) return new EmbedBuilder().Reply("Cleared queue");
            queue.Clear();
            return new EmbedBuilder().Reply("Cleared queue");
        }

        public async Task<EmbedBuilder> VolumeAsync(ulong guildId, int vol)
        {
            var player = _lava.GetPlayer(guildId);
            try
            {
                await player.SetVolumeAsync((uint) vol);
                return new EmbedBuilder().Reply($"Volume has been set to {vol}.");
            }
            catch (ArgumentException arg)
            {
                return new EmbedBuilder().Reply(arg.Message, Color.Red.RawValue);
            }
            catch
            {
                return new EmbedBuilder().Reply("Not playing anything currently.");
            }
        }

        public async Task<EmbedBuilder> SeekAsync(ulong guildId, TimeSpan span)
        {
            var player = _lava.GetPlayer(guildId);
            try
            {
                await player.SeekAsync(span.Milliseconds);
                return new EmbedBuilder().Reply($"**Seeked:** {player.CurrentTrack.Title}");
            }
            catch
            {
                return new EmbedBuilder().Reply("Not playing anything currently.");
            }
        }

        public EmbedBuilder Repeat(ulong guildId)
        {
            var repeat = _repeat.GetOrAdd(guildId, false);
            if (!repeat)
            {
                const bool newValue = true;
                _repeat.TryUpdate(guildId, newValue, repeat);
                return new EmbedBuilder().Reply("Looping queue!");
            }
            else
            {
                const bool newValue = false;
                _repeat.TryUpdate(guildId, newValue, repeat);
                return new EmbedBuilder().Reply("Stopped looping queue!");
            }
        }

        public async Task<EmbedBuilder> FixPlayer(ulong guildId, IVoiceState vc, IMessageChannel txC)
        {
            try
            {
                await _lava.LeaveAsync(guildId);
            }
            catch
            {
            }

            await Task.Delay(1000);

            await _lava.JoinAsync(vc.VoiceChannel);
            return new EmbedBuilder().Reply("Reconnected!");
        }

        public async Task<EmbedBuilder> SkipAsync(ulong guildId, ulong userId)
        {
            var player = _lava.GetPlayer(guildId);
            try
            {
                var users = (await player.VoiceChannel.GetUsersAsync().FlattenAsync()).Count(x => !x.IsBot);
                if (!_voteSkip.ContainsKey(guildId))
                    _voteSkip.TryAdd(guildId, (player.CurrentTrack, new List<ulong>()));
                _voteSkip.TryGetValue(guildId, out var skipInfo);

                if (!skipInfo.Votes.Contains(userId)) skipInfo.Votes.Add(userId);
                var perc = (int) Math.Round((double) (100 * skipInfo.Votes.Count) / users);
                if (perc <= 50) return new EmbedBuilder().Reply("More votes needed.");
                _voteSkip.TryUpdate(guildId, skipInfo, skipInfo);
                await player.StopAsync();
                return new EmbedBuilder().Reply($"**Skipped:** {player.CurrentTrack.Title}");
            }
            catch
            {
                return new EmbedBuilder().Reply("Not playing anything currently.", Color.Red.RawValue);
            }
        }

        public async Task ConnectAsync(ulong guildId, IVoiceState state, IMessageChannel channel)
        {
            if (state.VoiceChannel == null)
            {
                await channel.SendMessageAsync(null, false,
                    new EmbedBuilder().Reply("You aren't connected to any voice channels.", Color.Red.RawValue)
                        .Build());
                return;
            }

            await _lava.GetOrCreatePlayerAsync(guildId, state.VoiceChannel, channel);
            await channel.SendMessageAsync(null, false,
                new EmbedBuilder().Reply($"Connected to {state.VoiceChannel}.", Color.Green.RawValue).Build());
        }

        public async Task<EmbedBuilder> DisconnectAsync(ulong guildId)
        {
            try
            {
                await _lava.LeaveAsync(guildId);
                return new EmbedBuilder().Reply("Disconnected!", Color.Green.RawValue);
            }
            catch
            {
                return new EmbedBuilder().Reply("Can't leave when I'm not connected??", Color.Red.RawValue);
            }
        }

        private async Task<EmbedBuilder> QueueSongAsync(LavalinkPlayer player, LavalinkTrack track,
            ITextChannel channel)
        {
            var queue = _queue.GetOrAdd(channel.GuildId, new List<LavalinkTrack>());
            _textChannel.AddOrUpdate(channel.GuildId, channel, (arg1, textChannel) => channel);
            queue.Add(track);
            if (player.Playing) return new EmbedBuilder().Reply($"Queued: {track.Title}");
            await player.PlayAsync(track);
            return new EmbedBuilder().Reply($"Playing: {track.Title}");
        }

        private async Task OnFinished(LavalinkPlayer player, LavalinkTrack track, string arg3)
        {
            _queue.TryGetValue(player.VoiceChannel.GuildId, out var queue);
            var repeat = _repeat.GetOrAdd(player.VoiceChannel.GuildId, false);
            _textChannel.TryGetValue(player.VoiceChannel.GuildId, out var ch);
            if (queue == null)
            {
                await player.DisconnectAsync();
                if (ch != null)
                    await ch.SendMessageAsync(null, false, new EmbedBuilder().Reply("Queue Completed!").Build());
                return;
            }

            var queueIndex = _queNumber.GetOrAdd(player.VoiceChannel.GuildId, 0);
            if (repeat)
            {
                var index = queueIndex + 1;
                if (index == queue.Count) index = 0;
                var nextTrack = queue[index] ?? queue[0];
                _queNumber.AddOrUpdate(player.VoiceChannel.GuildId, index, (arg1, i) => index);
                await player.PlayAsync(nextTrack);
                if (ch != null)
                    await ch.SendMessageAsync(null, false,
                        new EmbedBuilder().Reply($"**Now Playing:** {nextTrack.Title}").Build());
            }
            else
            {
                queue.Remove(track);
                var nextTrack = queue[0];
                if (nextTrack == null)
                {
                    await player.DisconnectAsync();
                    if (ch != null)
                        await ch.SendMessageAsync(null, false, new EmbedBuilder().Reply("Queue Completed!").Build());
                    return;
                }

                await player.PlayAsync(nextTrack);
                if (ch != null)
                    await ch.SendMessageAsync(null, false,
                        new EmbedBuilder().Reply($"**Now Playing:** {nextTrack.Title}").Build());
            }
        }

        private async Task OnStuck(LavalinkPlayer player, LavalinkTrack track, long arg3)
        {
            await ResolveError(player, track);
        }

        private async Task OnException(LavalinkPlayer player, LavalinkTrack track, string arg3)
        {
            await ResolveError(player, track);
        }

        private async Task ResolveError(LavalinkPlayer player, LavalinkTrack track)
        {
            var queCheck = _queue.TryGetValue(player.VoiceChannel.GuildId, out var queue);
            if (!queCheck) return;
            LavalinkTrack nextTrack = null;
            var tries = 0;
            var index = 0;
            while (nextTrack == null)
            {
                if (tries >= MaxTries) return;
                if (queue.Count == 0) return;
                if (queue[index] != null) nextTrack = queue[index];
                tries++;
                index++;
            }

            await player.PlayAsync(nextTrack);
        }
    }
}