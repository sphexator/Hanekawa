﻿using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Objects;
using Victoria.Objects.Enums;

namespace Hanekawa.Modules.Audio.Service
{
    public class AudioService
    {
        private LavaNode _lavaNode;
        private ConcurrentDictionary<ulong, (LavaTrack Track, List<ulong> Votes)> _voteSkip;

        public void Initialize(LavaNode node)
        {
            _lavaNode = node;
            node.Stuck += OnStuck;
            node.Finished += OnFinished;
            node.Exception += OnException;
            _voteSkip = new ConcurrentDictionary<ulong, (LavaTrack Track, List<ulong> Votes)>();
        }

        public async Task<string> PlayAsync(ulong guildId, string query, IVoiceState state, IMessageChannel channel)
        {
            var search = Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute)
                ? await _lavaNode.GetTracksAsync(new Uri(query))
                : await _lavaNode.SearchYouTubeAsync(query);

            var track = search.Tracks.FirstOrDefault();
            var player = _lavaNode.GetPlayer(guildId);
            if (player == null)
            {
                player = await _lavaNode.JoinAsync(state.VoiceChannel, channel);
                player.Play(track);
                return $"**Playing:** {track.Title}";
            }
            if (player.CurrentTrack != null)
            {
                player.Enqueue(track);
                return $"**Enqueued:** {track.Title}";
            }

            player.Play(track);
            return $"**Playing:** {track.Title}";
        }

        public async Task<string> StopAsync(ulong guildId)
        {
            var leave = await _lavaNode.LeaveAsync(guildId);
            return leave ? "Disconnected!" : "Can't leave when I'm not connected??";
        }

        public string Pause(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                player.Pause();
                return $"**Paused:** {player.CurrentTrack.Title}";
            }
            catch
            {
                return "Not playing anything currently.";
            }
        }

        public string Resume(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                player.Resume();
                return $"**Resumed:** {player.CurrentTrack.Title}";
            }
            catch
            {
                return "Not playing anything currently.";
            }
        }

        public string DisplayQueue(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                return string.Join("\n", player.Queue[guildId].Select(x => $"=> {x.Title}")) ?? "Your queue is empty.";
            }
            catch
            {
                return "Your queue is empty.";
            }
        }

        public string Volume(ulong guildId, int vol)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                player.Volume(vol);
                return $"Volume has been set to {vol}.";
            }
            catch (ArgumentException arg)
            {
                return arg.Message;
            }
            catch
            {
                return "Not playing anything currently.";
            }
        }

        public string Seek(ulong guildId, TimeSpan span)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                player.Seek(span);
                return $"**Seeked:** {player.CurrentTrack.Title}";
            }
            catch
            {
                return "Not playing anything currently.";
            }
        }

        public async Task<string> SkipAsync(ulong guildId, ulong userId)
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
                if (perc <= 50) return "More votes needed.";
                _voteSkip.TryUpdate(guildId, skipInfo, skipInfo);
                player.Stop();
                return $"**Skipped:** {player.CurrentTrack.Title}";
            }
            catch
            {
                return "Not playing anything currently.";
            }
        }

        public async Task ConnectAsync(ulong guildId, IVoiceState state, IMessageChannel channel)
        {
            if (state.VoiceChannel == null)
            {
                await channel.SendMessageAsync("You aren't connected to any voice channels.");
                return;
            }

            var player = await _lavaNode.JoinAsync(state.VoiceChannel, channel);
            player.Queue.TryAdd(guildId, new LinkedList<LavaTrack>());
            await channel.SendMessageAsync($"Connected to {state.VoiceChannel}.");
        }

        public async Task<string> DisconnectAsync(ulong guildId)
            => await _lavaNode.LeaveAsync(guildId) ? "Disconnected." : "Not connected to any voice channels.";

        private void OnFinished(LavaPlayer player, LavaTrack track, TrackReason reason)
        {
            player.Queue.TryGetValue(player.Guild.Id, out var queue);
            var nextTrack = queue.First?.Value ?? queue.First.Next?.Value;
            player.Dequeue(track);
            if (nextTrack == null)
            {
                _lavaNode.LeaveAsync(player.Guild.Id).GetAwaiter().GetResult();
                player.TextChannel.SendMessageAsync("Queue Completed!").GetAwaiter().GetResult();
                return;
            }

            player.Play(nextTrack);
            player.TextChannel.SendMessageAsync($"**Now Playing:** {track.Title}");
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
