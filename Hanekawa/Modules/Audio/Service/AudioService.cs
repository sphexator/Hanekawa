using Discord;
using Discord.WebSocket;
using Hanekawa.Entities;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;
using Victoria.Entities.Enums;

namespace Hanekawa.Modules.Audio.Service
{
    public class AudioService : IHanaService, IRequiredService
    {
        private const int MaxTries = 100;
        private readonly DiscordSocketClient _client;
        private LavaNode _lavaNode;
        private readonly Lavalink _lavaLink;
        private readonly LogService _log;

        private readonly Lazy<Random> _lazyRandom
            = new Lazy<Random>();

        private readonly Lazy<ConcurrentDictionary<ulong, AudioOptions>> _lazyOptions
            = new Lazy<ConcurrentDictionary<ulong, AudioOptions>>();

        private Random Random
            => _lazyRandom.Value;

        private ConcurrentDictionary<ulong, AudioOptions> Options
            => _lazyOptions.Value;

        public AudioService(DiscordSocketClient client, Lavalink lavaLink, LogService log)
        {
            _client = client;
            _lavaLink = lavaLink;
            _log = log;

            _client.Ready += OnReady;

            Console.WriteLine("Audio service loaded");
        }

        private async Task OnReady()
        {
            _lavaNode = await _lavaLink.AddNodeAsync(_client, new Configuration
            {
                Port = 2333,
                Host = "127.0.0.1",
                BufferSize = 2048,
                ReconnectAttempts = -1,
                ReconnectInterval = TimeSpan.FromSeconds(3),
                Severity = LogSeverity.Info,
                SelfDeaf = false,
                Authorization = "youshallnotpass",
                NodePrefix = "LavaNode_",
                Proxy = default
            });

            _lavaNode.TrackFinished = OnFinished;
            _lavaNode.TrackException = OnException;
            _lavaNode.TrackStuck = OnStuck;
        }

        public async Task<EmbedBuilder> PlayAsync(ulong guildId, string query, ITextChannel channel, IVoiceState state)
        {
            var node = _lavaLink.DefaultNode;
            try
            {
                var player = await node.GetOrCreatePlayerAsync(guildId, state.VoiceChannel, channel);
                LavaTrack track;

                if (string.IsNullOrWhiteSpace(query))
                {
                    if (player.Queue.Count < 1)
                        return new EmbedBuilder().CreateDefault("Queue is empty. Please queue something first.", channel.GuildId);
                    track = player.Queue.Dequeue();
                }
                else
                {
                    var search = await node.SearchYouTubeAsync(query)
                                 ?? await node.GetTracksAsync(query);

                    if (search.LoadResultType == LoadResultType.NoMatches)
                        return new EmbedBuilder().CreateDefault($"I wasn't able to find anything for {query}.", channel.GuildId);

                    track = search.Tracks.FirstOrDefault();
                }

                if (node.IsConnected && !(player.CurrentTrack is null))
                {
                    player.Queue.Enqueue(track);
                    return new EmbedBuilder().CreateDefault($"{track.Title} has been added to queue.", channel.GuildId);
                }

                await player.PlayAsync(track);
                return new EmbedBuilder().CreateDefault($"**Now Playing:** {track.Title}", guildId);
            }
            catch (Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().CreateDefault("Couldn't play that", guildId); ;
            }
        }

        public async Task<EmbedBuilder> StopAsync(ulong guildId)
        {
            var node = _lavaLink.DefaultNode;
            try
            {
                var player = node.GetPlayer(guildId);
                await player.StopAsync();
                await node.DisconnectAsync(guildId);
                return new EmbedBuilder().CreateDefault("Disconnected!", Color.Green.RawValue);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().CreateDefault("Can't leave when I'm not connected??", Color.Red.RawValue);
            }
        }

        public async Task<EmbedBuilder> PauseAsync(ulong guildId)
        {
            var node = _lavaLink.DefaultNode;
            var player = node.GetPlayer(guildId);
            try
            {
                await player.PauseAsync();
                return new EmbedBuilder().CreateDefault($"**Paused:** {player.CurrentTrack.Title}", guildId);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().CreateDefault("Not playing anything currently.", guildId);
            }
        }

        public async Task<EmbedBuilder> ResumeAsync(ulong guildId)
        {
            var node = _lavaLink.DefaultNode;
            var player = node.GetPlayer(guildId);
            try
            {
                await player.PauseAsync();
                return new EmbedBuilder().CreateDefault($"**Resumed:** {player.CurrentTrack.Title}", Color.Green.RawValue);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().CreateDefault("Not playing anything currently.", Color.Red.RawValue);
            }
        }

        public EmbedBuilder DisplayQueue(ulong guildId)
        {
            var node = _lavaLink.DefaultNode;
            var player = node.GetPlayer(guildId);
            var embed = new EmbedBuilder().CreateDefault(guildId);
            try
            {
                if (player.IsPlaying && player.CurrentTrack != null)
                {
                    embed.Title = player.CurrentTrack.Title;
                    embed.Url = player.CurrentTrack.Uri.AbsolutePath;
                }

                string queue = null;
                var limit = 10;
                if (player.Queue != null && player.Queue.Count < limit) limit = player.Queue.Count;
                if (player.Queue != null && player.Queue.Count > 0)
                {
                    var tries = 0;
                    foreach (var x in player.Queue.Items)
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
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().CreateDefault("Queue is empty.", guildId);
            }
        }

        public async Task<EmbedBuilder> ClearQueueAsync(ulong guildId)
        {
            var node = _lavaLink.DefaultNode;
            var player = node.GetPlayer(guildId);
            if (player == null) return new EmbedBuilder().CreateDefault("Not playing anything currently.", Color.Red.RawValue);
            if (player.CurrentTrack != null) await player.StopAsync();
            if (player.Queue == null || player.Queue.Count == 0) return new EmbedBuilder().CreateDefault("No queue", guildId);
            player.Queue.Clear();
            return new EmbedBuilder().CreateDefault("Cleared queue", guildId);
        }

        public async Task<EmbedBuilder> VolumeAsync(ulong guildId, int vol)
        {
            var node = _lavaLink.DefaultNode;
            var player = node.GetPlayer(guildId);
            try
            {
                await player.SetVolumeAsync(vol);
                return new EmbedBuilder().CreateDefault($"Volume has been set to {vol}.", guildId);
            }
            catch (ArgumentException arg)
            {
                return new EmbedBuilder().CreateDefault(arg.Message, Color.Red.RawValue);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().CreateDefault("Not playing anything currently.", guildId);
            }
        }

        public async Task<EmbedBuilder> SeekAsync(ulong guildId, TimeSpan span)
        {
            var node = _lavaLink.DefaultNode;
            var player = node.GetPlayer(guildId);
            try
            {
                await player.SeekAsync(span);
                return new EmbedBuilder().CreateDefault($"**Seeked:** {player.CurrentTrack.Title}", guildId);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().CreateDefault("Not playing anything currently.", guildId);
            }
        }

        public async Task<EmbedBuilder> FixPlayer(ulong guildId, IVoiceState vc, IMessageChannel txC)
        {
            var node = _lavaLink.DefaultNode;
            try
            {
                var player = node.GetPlayer(guildId);
                if (player != null) await player.StopAsync();
                await node.DisconnectAsync(guildId);
            }
            catch
            {
                // ignored
            }

            await Task.Delay(1000);

            await node.ConnectAsync(vc.VoiceChannel, txC);
            return new EmbedBuilder().CreateDefault("Reconnected!", vc.VoiceChannel.GuildId);
        }

        public async Task<EmbedBuilder> SkipAsync(ulong guildId, SocketGuildUser user)
        {
            var node = _lavaLink.DefaultNode;
            try
            {
                var player = node.GetPlayer(guildId);
                Options.TryGetValue(guildId, out var options);

                if (options.Voters.Contains(user.Id))
                    return new EmbedBuilder().CreateDefault("You've already voted. Please don't vote again.", guildId);

                options.VotedTrack = player.Queue.Peek();
                options.Voters.Add(user.Id);
                var perc = options.Voters.Count / user.VoiceChannel.Users.Count(x => !x.IsBot) * 100;

                if (perc < 60)
                    return new EmbedBuilder().CreateDefault("More votes needed.", guildId);

                var track = player.CurrentTrack;
                await player.SkipAsync();
                options.VotedTrack = null;
                options.Voters.Clear();

                return new EmbedBuilder().CreateDefault($"$**Skipped:** {track.Title}", guildId);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().CreateDefault("Not playing anything currently.", Color.Red.RawValue);
            }
        }

        public EmbedBuilder LoopPlayer(ulong guildId)
        {
            Options.TryGetValue(guildId, out var options);
            var embed = new EmbedBuilder();
            if (options.Loop)
            {
                options.Loop = false;
                Options.AddOrUpdate(guildId, options, (key, audio) => options);
                embed.CreateDefault("No longer looping playlist", guildId);
            }
            else
            {
                options.Loop = true;
                Options.AddOrUpdate(guildId, options, (key, audio) => options);
                embed.CreateDefault("Looping playlist", guildId);
            }
            return embed;
        }

        public EmbedBuilder Repeat(ulong guildId)
        {
            Options.TryGetValue(guildId, out var options);
            var embed = new EmbedBuilder();
            if (options.RepeatTrack)
            {
                options.RepeatTrack = false;
                Options.AddOrUpdate(guildId, options, (key, audio) => options);
                embed.CreateDefault("No longer repeating Track", guildId);
            }
            else
            {
                options.RepeatTrack = true;
                Options.AddOrUpdate(guildId, options, (key, audio) => options);
                embed.CreateDefault("Repeating track", guildId);
            }
            return embed;
        }

        public async Task ConnectAsync(ulong guildId, IVoiceState state, IMessageChannel channel)
        {
            var node = _lavaLink.DefaultNode;
            if (state.VoiceChannel == null)
            {
                await channel.SendMessageAsync(null, false,
                    new EmbedBuilder().CreateDefault("You aren't connected to any voice channels.", Color.Red.RawValue)
                        .Build());
                return;
            }

            await node.GetOrCreatePlayerAsync(guildId, state.VoiceChannel, channel);
            await channel.SendMessageAsync(null, false,
                new EmbedBuilder().CreateDefault($"Connected to {state.VoiceChannel}.", Color.Green.RawValue).Build());
        }

        public async Task<EmbedBuilder> DisconnectAsync(ulong guildId)
        {
            var node = _lavaLink.DefaultNode;
            try
            {
                await node.GetPlayer(guildId).StopAsync();
                await node.DisconnectAsync(guildId);
                return new EmbedBuilder().CreateDefault("Disconnected!", Color.Green.RawValue);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().CreateDefault("Can't leave when I'm not connected", Color.Red.RawValue);
            }
        }

        private void Log(Exception ex)
        {
            _log.LogAction(LogLevel.Error, $"{ex.Message}\n{ex.StackTrace}", "AudioService");
        }

        private async Task OnException(LavaPlayer player, LavaTrack track, string error)
        {
            player.Queue.TryDequeue(out var nextTrack);
            if (nextTrack is null)
            {
                await player.StopAsync();
            }
            else
            {
                await player.PlayAsync(nextTrack);
            }
        }

        private async Task OnStuck(LavaPlayer player, LavaTrack track, long threshold)
        {
            player.Queue.TryDequeue(out var nextTrack);
            if (nextTrack is null)
            {
                await player.StopAsync();
            }
            else
            {
                await player.PlayAsync(nextTrack);
            }
        }

        private async Task OnFinished(LavaPlayer player, LavaTrack track, TrackReason reason)
        {
            if (reason is TrackReason.LoadFailed || reason is TrackReason.Cleanup)
                return;

            LavaTrack nextTrack;
            Options.TryGetValue(player.VoiceChannel.GuildId, out var options);

            if (options.Loop)
            {
                var search = await _lavaLink.DefaultNode.GetTracksAsync(track.Title);
                nextTrack = search.Tracks.FirstOrDefault();
            }
            else if (options.Shuffle)
            {
                player.Queue.Dequeue();
                player.Queue.Shuffle();
                player.Queue.TryDequeue(out nextTrack);
            }
            else
                player.Queue.TryDequeue(out nextTrack);

            if (nextTrack is null)
            {
                await player.StopAsync();
            }
            else
            {
                await player.PlayAsync(nextTrack);
            }
        }

    }
}