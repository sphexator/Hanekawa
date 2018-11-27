﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Entities;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Hanekawa.Services.Logging;
using Microsoft.Extensions.Logging;
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
            _lavaNode = await _lavaLink.AddNodeAsync(_client);
        }

        public async Task<EmbedBuilder> PlayAsync(ulong guildId, string query, ITextChannel channel, IVoiceState state)
        {
            try
            {
                var player = await _lavaNode.GetOrCreatePlayerAsync(guildId, state.VoiceChannel, channel);
                LavaTrack track;

                if (string.IsNullOrWhiteSpace(query))
                {
                    if (player.Queue.Count < 1)
                        return new EmbedBuilder().Reply("Queue is empty. Please queue something first.");
                    track = player.Queue.Dequeue();
                }
                else
                {
                    var search = await _lavaNode.SearchYouTubeAsync(query)
                                 ?? await _lavaNode.GetTracksAsync(query);

                    if (search.LoadResultType == LoadResultType.NoMatches)
                        return new EmbedBuilder().Reply($"I wasn't able to find anything for {query}.");

                    track = search.Tracks.FirstOrDefault();
                }

                if (_lavaNode.IsConnected && !(player.CurrentTrack is null))
                {
                    player.Queue.Enqueue(track);
                    return new EmbedBuilder().Reply($"{track.Title} has been added to queue.");
                }

                await player.PlayAsync(track);
                return new EmbedBuilder().Reply($"**Now Playing:** {track.Title}");
            }
            catch (Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().Reply("Couldn't play that"); ;
            }
        }

        public async Task<EmbedBuilder> StopAsync(ulong guildId)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guildId);
                await player.StopAsync();
                await _lavaNode.DisconnectAsync(guildId);
                return new EmbedBuilder().Reply("Disconnected!", Color.Green.RawValue);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().Reply("Can't leave when I'm not connected??", Color.Red.RawValue);
            }
        }

        public async Task<EmbedBuilder> PauseAsync(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                await player.PauseAsync();
                return new EmbedBuilder().Reply($"**Paused:** {player.CurrentTrack.Title}");
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().Reply("Not playing anything currently.");
            }
        }

        public async Task<EmbedBuilder> ResumeAsync(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                await player.ResumeAsync();
                return new EmbedBuilder().Reply($"**Resumed:** {player.CurrentTrack.Title}", Color.Green.RawValue);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().Reply("Not playing anything currently.", Color.Red.RawValue);
            }
        }

        public EmbedBuilder DisplayQueue(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            var embed = new EmbedBuilder {Color = Color.Purple};
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
                return new EmbedBuilder().Reply("Queue is empty.");
            }
        }

        public async Task<EmbedBuilder> ClearQueueAsync(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if (player == null) return new EmbedBuilder().Reply("Not playing anything currently.", Color.Red.RawValue);
            if (player.CurrentTrack != null) await player.StopAsync();
            if (player.Queue == null || player.Queue.Count == 0) return new EmbedBuilder().Reply("No queue");
            player.Queue.Clear();
            return new EmbedBuilder().Reply("Cleared queue");
        }

        public async Task<EmbedBuilder> VolumeAsync(ulong guildId, int vol)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                await player.SetVolumeAsync(vol);
                return new EmbedBuilder().Reply($"Volume has been set to {vol}.");
            }
            catch (ArgumentException arg)
            {
                return new EmbedBuilder().Reply(arg.Message, Color.Red.RawValue);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().Reply("Not playing anything currently.");
            }
        }

        public async Task<EmbedBuilder> SeekAsync(ulong guildId, TimeSpan span)
        {
            var player = _lavaNode.GetPlayer(guildId);
            try
            {
                await player.SeekAsync(span);
                return new EmbedBuilder().Reply($"**Seeked:** {player.CurrentTrack.Title}");
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().Reply("Not playing anything currently.");
            }
        }

        public async Task<EmbedBuilder> FixPlayer(ulong guildId, IVoiceState vc, IMessageChannel txC)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guildId);
                if (player != null) await player.StopAsync();
                await _lavaNode.DisconnectAsync(guildId);
            }
            catch
            {
                // ignored
            }

            await Task.Delay(1000);

            await _lavaNode.ConnectAsync(vc.VoiceChannel, txC);
            return new EmbedBuilder().Reply("Reconnected!");
        }

        public async Task<EmbedBuilder> SkipAsync(ulong guildId, SocketGuildUser user)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guildId);
                Options.TryGetValue(guildId, out var options);

                if (options.Voters.Contains(user.Id))
                    return new EmbedBuilder().Reply("You've already voted. Please don't vote again.");

                options.VotedTrack = player.Queue.Peek();
                options.Voters.Add(user.Id);
                var perc = options.Voters.Count / user.VoiceChannel.Users.Count(x => !x.IsBot) * 100;

                if (perc < 60)
                    return new EmbedBuilder().Reply("More votes needed.");

                var track = player.CurrentTrack;
                await player.SkipAsync();
                options.VotedTrack = null;
                options.Voters.Clear();

                return new EmbedBuilder().Reply($"$**Skipped:** {track.Title}");
            }
            catch(Exception ex)
            {
                Log(ex);
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

            await _lavaNode.GetOrCreatePlayerAsync(guildId, state.VoiceChannel, channel);
            await channel.SendMessageAsync(null, false,
                new EmbedBuilder().Reply($"Connected to {state.VoiceChannel}.", Color.Green.RawValue).Build());
        }

        public async Task<EmbedBuilder> DisconnectAsync(ulong guildId)
        {
            try
            {
                await _lavaNode.GetPlayer(guildId).StopAsync();
                await _lavaNode.DisconnectAsync(guildId);
                return new EmbedBuilder().Reply("Disconnected!", Color.Green.RawValue);
            }
            catch(Exception ex)
            {
                Log(ex);
                return new EmbedBuilder().Reply("Can't leave when I'm not connected", Color.Red.RawValue);
            }
        }

        private void Log(Exception ex)
        {
            _log.LogAction(LogLevel.Error, $"{ex.Message}\n{ex.StackTrace}", "AudioService");
        }
    }
}