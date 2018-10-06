using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Victoria;
using Victoria.Objects;
using Victoria.Objects.Enums;

namespace Hanekawa.Modules.Audio.Service
{
    public class AudioService
    {
        private LavaNode _lavaNode;
        private readonly DiscordSocketClient _client;
        private readonly Lavalink _lavalink;
        private ConcurrentDictionary<ulong, (LavaTrack Track, List<ulong> Votes)> _voteSkip;
        private ConcurrentDictionary<ulong, bool> _repeat;

        public AudioService(Lavalink lavalink, DiscordSocketClient client)
        {
            _lavalink = lavalink;
            _client = client;

            _client.Ready += OnReady;
        }

        private async Task OnReady()
        {
            var node = await _lavalink.ConnectAsync(_client);
            Initialize(node);
        }

        public void Initialize(LavaNode node)
        {
            _lavaNode = node;
            node.Stuck += OnStuck;
            node.Finished += OnFinished;
            node.Exception += OnException;
            _voteSkip = new ConcurrentDictionary<ulong, (LavaTrack Track, List<ulong> Votes)>();
            _repeat = new ConcurrentDictionary<ulong, bool>();
        }

        public async Task<EmbedBuilder> PlayAsync(IGuildUser user, string query, IVoiceState state, IMessageChannel channel)
        {
            var player = await _lavaNode.GetOrCreatePlayerAsync(user.GuildId, state.VoiceChannel, channel);
            var track = (await _lavaNode.GetSongAsync(query)).Tracks.FirstOrDefault();
            if (track == null)
            {
                return new EmbedBuilder
                {
                    Color = Color.Purple,
                    Description = $"{user.Mention}, Couldn't play song"
                };
            }
            switch (player.CurrentTrack)
            {
                case null when player.Queue.IsEmpty:
                    player.Play(track);
                    player.Enqueue(track);
                    return new EmbedBuilder().Reply($"Playing: {track.Title}");
                case null:
                    player.Play(track);
                    player.Enqueue(track);
                    return new EmbedBuilder().Reply($"Playing: {track.Title}");
                default:
                    player.Enqueue(track);
                    return new EmbedBuilder().Reply($"Queued: {track.Title}");
            }
        }

        public async Task<EmbedBuilder> StopAsync(ulong guildId)
        {
            var leave = await _lavaNode.LeaveAsync(guildId);
            return leave ? new EmbedBuilder().Reply("Disconnected!", Color.Green.RawValue) : new EmbedBuilder().Reply("Can't leave when I'm not connected??", Color.Red.RawValue);
        }

        public EmbedBuilder Pause(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                player.Pause();
                return new EmbedBuilder().Reply($"**Paused:** {player.CurrentTrack.Title}");
            }
            catch
            {
                return new EmbedBuilder().Reply("Not playing anything currently.");
            }
        }

        public EmbedBuilder Resume(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                player.Resume();
                return new EmbedBuilder().Reply($"**Resumed:** {player.CurrentTrack.Title}", Color.Green.RawValue);
            }
            catch
            {
                return new EmbedBuilder().Reply("Not playing anything currently.", Color.Red.RawValue);
            }
        }

        public EmbedBuilder DisplayQueue(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                return new EmbedBuilder().Reply(string.Join("\n", player.Queue[guildId].Select(x => $"=> {x.Title}")) ?? "Your queue is empty.");
            }
            catch
            {
                return new EmbedBuilder().Reply("Your queue is empty.");
            }
        }

        public EmbedBuilder Volume(ulong guildId, int vol)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                player.Volume(vol);
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

        public EmbedBuilder Seek(ulong guildId, TimeSpan span)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                player.Seek(span);
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
                var newValue = false;
                _repeat.TryUpdate(guildId, newValue, repeat);
                return new EmbedBuilder().Reply("Stopped looping queue!");
            }
        }

        public async Task<EmbedBuilder> FixPlayer(ulong guildId, IVoiceState vc, IMessageChannel txC)
        {
            if (!_lavaNode.IsConnected)
            {
                await OnReconnect();
            }
            try {  await _lavaNode.LeaveAsync(guildId); } catch { }

            await Task.Delay(1000);

            await _lavaNode.JoinAsync(vc.VoiceChannel, txC);
            return new EmbedBuilder().Reply("Reconnected!");
        }

        public async Task<EmbedBuilder> SkipAsync(ulong guildId, ulong userId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                var users = (await player.VoiceChannel.GetUsersAsync().FlattenAsync()).Count(x => !x.IsBot);
                if (!_voteSkip.ContainsKey(guildId))
                    _voteSkip.TryAdd(guildId, (player.CurrentTrack, new List<ulong>()));
                _voteSkip.TryGetValue(guildId, out var skipInfo);

                if (!skipInfo.Votes.Contains(userId)) skipInfo.Votes.Add(userId);
                var perc = (int)Math.Round((double)(100 * skipInfo.Votes.Count) / users);
                if (perc <= 50) return new EmbedBuilder().Reply("More votes needed.");
                _voteSkip.TryUpdate(guildId, skipInfo, skipInfo);
                player.Stop();
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
                await channel.SendMessageAsync(null, false, new EmbedBuilder().Reply("You aren't connected to any voice channels.", Color.Red.RawValue).Build());
                return;
            }

            var player = await _lavaNode.JoinAsync(state.VoiceChannel, channel);
            player.Queue.TryAdd(guildId, new LinkedList<LavaTrack>());
            await channel.SendMessageAsync(null, false, new EmbedBuilder().Reply($"Connected to {state.VoiceChannel}.", Color.Green.RawValue).Build());
        }

        public async Task<EmbedBuilder> DisconnectAsync(ulong guildId)
            => await _lavaNode.LeaveAsync(guildId) ? new EmbedBuilder().Reply("Disconnected"): new EmbedBuilder().Reply("Not connected to any voice channels.", Color.Red.RawValue);

        private async Task OnReconnect() => await _lavalink.ConnectAsync(_client);

        private void OnFinished(LavaPlayer player, LavaTrack track, TrackReason reason)
        {
            player.Queue.TryGetValue(player.Guild.Id, out var queue);
            var repeat = _repeat.GetOrAdd(player.Guild.Id, false);
            if (queue == null)
            {
                _lavaNode.LeaveAsync(player.Guild.Id).GetAwaiter().GetResult();
                player.TextChannel.SendMessageAsync(null, false, new EmbedBuilder().Reply("Queue Completed!").Build()).GetAwaiter().GetResult();
                return;
            }
            if (repeat)
            {
                var nextTrack = queue.First.Next;
                player.Dequeue(track);
                if (track == queue.Last.Value) nextTrack = queue.First;
                player.Play(nextTrack == null ? track : nextTrack.Value);
            }
            else
            {
                var nextTrack = queue.First.Next;
                player.Dequeue(track);
                if (nextTrack == null)
                {
                    _lavaNode.LeaveAsync(player.Guild.Id).GetAwaiter().GetResult();
                    player.TextChannel.SendMessageAsync(null, false, new EmbedBuilder().Reply("Queue Completed!").Build()).GetAwaiter().GetResult();
                    return;
                }
                player.Play(nextTrack.Value);
                player.TextChannel.SendMessageAsync(null, false, new EmbedBuilder().Reply($"**Now Playing:** {track.Title}").Build());
            }
        }

        private static void OnStuck(LavaPlayer player, LavaTrack track, long arg3)
            => ResolveError(player, track, 's');

        private static void OnException(LavaPlayer player, LavaTrack track, string arg3)
            => ResolveError(player, track, 'e');

        private static void ResolveError(LavaPlayer player, LavaTrack track, char errorType)
        {
            player.Dequeue(track);
            player.Enqueue(track);
        }
    }
}