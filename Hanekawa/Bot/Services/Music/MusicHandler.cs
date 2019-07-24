using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared;
using Victoria.Entities;

namespace Hanekawa.Bot.Services.Music
{
    public partial class MusicService
    {
        public async Task PlayAsync(SocketGuildUser user, SocketTextChannel channel, SocketVoiceChannel vc, string url)
        {
            var options = _audioOptions.GetOrAdd(user.Guild.Id, new AudioOption());
            if (options.Mode == MusicMode.Karaoke)
            {
                await PlayAsync(channel, vc, url, user);
                return;
            }

            var player = _lavaClient.GetPlayer(user.Guild.Id) ?? await _lavaClient.ConnectAsync(vc, channel);
            var search = await _lavaRest.SearchYouTubeAsync(url);
            if (search.LoadType == LoadType.LoadFailed || search.LoadType == LoadType.NoMatches)
            {
                await channel.ReplyAsync("Couldn't find any songs with that name or url", Color.Red.RawValue);
                return;
            }

            var track = search.Tracks.FirstOrDefault();

            if (player.IsPlaying)
            {
                player.Queue.Enqueue(track);
                await channel.ReplyAsync($"{track.Title} has been queued.", user.Guild.Id);
            }
            else
            {
                await player.PlayAsync(track);
                await channel.ReplyAsync($"Now Playing: {track.Title}", user.Guild.Id);
            }
        }

        public async Task SkipAsync(SocketVoiceChannel vc) => await _lavaClient.DisconnectAsync(vc);

        public async Task<bool> SetVolumeAsync(SocketGuild guild, int volume)
        {
            var player = _lavaClient.GetPlayer(guild.Id);
            if (player == null) return false;
            await player.SetVolumeAsync(volume);
            return true;
        }

        public async Task<bool> SetPositionAsync(SocketGuildUser user, TimeSpan position)
        {
            var player = _lavaClient.GetPlayer(user.Guild.Id);
            if (player == null) return false;
            await player.SeekAsync(position);
            return true;
        }

        public async Task MoveAsync(SocketVoiceChannel voice, SocketTextChannel text)
        {
            var player = _lavaClient.GetPlayer(voice.Guild.Id);
            if (player == null) await _lavaClient.ConnectAsync(voice, text);
            else await _lavaClient.MoveChannelsAsync(voice);
        }
    }
}