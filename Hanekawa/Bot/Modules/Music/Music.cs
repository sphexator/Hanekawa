using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Qmmands;
using Quartz.Util;
using Victoria;
using Victoria.Entities;

namespace Hanekawa.Bot.Modules.Music
{
    [Name("Music")]
    [Description("Music module")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    [RequiredMusicChannel, RequiredChannel]
    public class Music : InteractiveBase
    {
        private readonly LavaRestClient _lavaRestClient;
        private readonly LavaSocketClient _lavaSocketClient;
        private readonly RequiredMusicChannel _channel;

        public Music(LavaSocketClient lavaSocketClient, LavaRestClient lavaRestClient, RequiredMusicChannel channel)
        {
            _lavaSocketClient = lavaSocketClient;
            _lavaRestClient = lavaRestClient;
            _channel = channel;
        }

        [Name("Join")]
        [Command("join")]
        [Description("Joins the voice channel")]
        public async Task JoinAsync()
        {
            if (Context.User.VoiceChannel == null)
            {
                await Context.ReplyAsync("You need to be in a voice channel to use this command", Color.Red.RawValue);
                return;
            }

            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null)
            {
                player = await _lavaSocketClient.ConnectAsync(Context.User.VoiceChannel, Context.Channel);
                await Context.ReplyAsync($"Connected to {player.VoiceChannel.Name}", Color.Red.RawValue);
            }
            else
            {
                await Context.ReplyAsync($"Already connected to {player.VoiceChannel.Name}", Color.Red.RawValue);
            }
        }

        [Name("Move")]
        [Command("move")]
        [Description("Moves the bot to a different voice channel")]
        public async Task MoveAsync()
        {
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null)
            {
                await _lavaSocketClient.ConnectAsync(Context.User.VoiceChannel, Context.Channel);
                await Context.ReplyAsync($"Connected to {Context.User.VoiceChannel.Name}");
                return;
            }
            if (player.IsPlaying || Context.User.VoiceChannel == player.VoiceChannel) return;
            
            var old = player.VoiceChannel; 
            await _lavaSocketClient.MoveChannelsAsync(Context.User.VoiceChannel); 
            await Context.ReplyAsync($"Moved from {old.Name} to {Context.User.VoiceChannel}");
        }

        [Name("Stop")]
        [Command("stop")]
        [Description("stops the current song from playing")]
        public async Task StopAsync()
        {
            if (Context.User.VoiceChannel == null) return;
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if(player == null) return;
            if (Context.User.VoiceChannel != player.VoiceChannel) return;
            var song = player.CurrentTrack;
            await player.StopAsync();
            if (song.Title.IsNullOrWhiteSpace())
            {
                await Context.ReplyAsync("Stopped current player", Color.Green.RawValue);
                return;
            }
            await Context.ReplyAsync($"Stopped {song.Title} from playing.", Color.Green.RawValue);
        }

        [Name("Disconnect")]
        [Command("disconnect", "disc")]
        [Description("Disconnects the bot from voice channel.")]
        public async Task DisconnectAsync()
        {
            if (Context.User.VoiceChannel == null) return;
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null) return;
            if (Context.User.VoiceChannel != player.VoiceChannel) return;
            var song = player.CurrentTrack;
            await _lavaSocketClient.DisconnectAsync(player.VoiceChannel);
            await Context.ReplyAsync("Disconnected", Color.Green.RawValue);

        }

        [Name("Currently Playing")]
        [Command("CurrentlyPlaying", "NowPlaying", "np")]
        [Description("Display which song is currently playing")]
        public async Task NowPlayingAsync()
        {
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null || !player.IsPlaying)
            {
                await Context.ReplyAsync("Currently not playing");
                return;
            }

            await Context.ReplyAsync($"Currently playing: {player.CurrentTrack.Title}");
        }

        [Name("Play")]
        [Command("play")]
        [Description("Queues up a song, joins channel if not currently connected")]
        public async Task PlayAsync([Remainder] string query)
        {
            if (Context.User.VoiceChannel == null)
            {
                await Context.ReplyAsync("You need to be in a voice channel to use this command", Color.Red.RawValue);
                return;
            }

            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id) ??
                         await _lavaSocketClient.ConnectAsync(Context.User.VoiceChannel, Context.Channel);
            if (Context.User.VoiceChannel == null || player.VoiceChannel != Context.User.VoiceChannel) return;
            var songResult = await _lavaRestClient.SearchYouTubeAsync(query);
            if (songResult.LoadType == LoadType.NoMatches || songResult.LoadType == LoadType.LoadFailed)
            {
                await Context.ReplyAsync("Couldn't find a song with that name", Color.Red.RawValue);
            }
            else
            {
                var song = songResult.Tracks.FirstOrDefault();
                var msg = player.Queue.Count == 0 ? $"Started playing {song?.Title}" : $"Queued {song?.Title}";
                await player.PlayAsync(song);
                _lavaSocketClient.UpdateTextChannel(Context.Guild.Id, Context.Channel);
                await Context.ReplyAsync(msg);
            }
        }

        [Name("Queue")]
        [Command("queue", "que", "q")]
        [Description("Displays the current queue")]
        public async Task QueueAsync()
        {
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null) return;
            if (player.Queue.Count == 0)
            {
                await Context.ReplyAsync("Queue is currently empty");
                return;
            }

            var queue = new List<string>();
            for (var i = 0; i < player.Queue.Count; i++)
            {
                var x = player.Queue.Items.ElementAt(i);
                queue.Add(i == 0 ? $"{i + 1}> {x.Id}" : $"{i + 1}: {x.Id}");
            }

            await PagedReplyAsync(queue.PaginateBuilder(Context.Guild, $"Current queue in {Context.Guild.Name}", null));
        }

        [Name("Pause")]
        [Command("pause")]
        [Description("Pauses the player if its currently playing")]
        public async Task PauseAsync()
        {
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null || player.IsPaused) return;
            if (Context.User.VoiceChannel == null || player.VoiceChannel != Context.User.VoiceChannel) return;
            await player.PauseAsync();
            await Context.ReplyAsync("Paused the song!");
        }

        [Name("Resume")]
        [Command("resume")]
        [Description("Resumes the player if its paused")]
        public async Task ResumeAsync()
        {
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null || !player.IsPaused) return;
            if (Context.User.VoiceChannel == null || player.VoiceChannel != Context.User.VoiceChannel) return;
            await player.ResumeAsync();
            await Context.ReplyAsync("Resumed the song!");
        }

        [Name("Volume")]
        [Command("volume")]
        [Description("Sets volume")]
        public async Task VolumeAsync(int volume)
        {
            if (volume < 0) return;
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null) return;
            if (Context.User.VoiceChannel == null || player.VoiceChannel != Context.User.VoiceChannel) return;
            await player.SetVolumeAsync(volume);
            await Context.ReplyAsync($"Set volume to {volume}%");
        }

        [Name("Skip")]
        [Command("skip")]
        [Description("Skips current song")]
        public async Task SkipAsync()
        {
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null) return;
            if (Context.User.VoiceChannel == null || player.VoiceChannel != Context.User.VoiceChannel) return;
            var track = await player.SkipAsync();
            await Context.ReplyAsync($"Skipped {track.Title}");
        }

        [Name("Clear")]
        [Command("clear")]
        [Description("Clears current playlist")]
        public async Task ClearAsync()
        {
            var player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            if (player == null) return;
            if (Context.User.VoiceChannel == null || player.VoiceChannel != Context.User.VoiceChannel) return;
            player.Queue.Clear();
            await player.StopAsync();
            await Context.ReplyAsync("Cleared queue");
        }

        [Name("Music Channel")]
        [Command("mc", "musicchannel")]
        [Description(
            "Sets a channel to be used to queue and play music, if none set, defaults to regular ignore channel")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task MusicChannelAsync(SocketTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateMusicConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.TextChId = null;
                    await db.SaveChangesAsync();
                    _channel.AddOrRemoveChannel(Context.Guild);
                    await Context.ReplyAsync("Disabled music channel. Following regular ignore channels");
                }
                else
                {
                    cfg.TextChId = channel.Id; 
                    _channel.AddOrRemoveChannel(Context.Guild, channel);
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Set music channel to {channel.Mention}");
                }
            }
        }
    }
}