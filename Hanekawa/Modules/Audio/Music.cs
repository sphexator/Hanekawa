using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Hanekawa.Services.Audio;
using Quartz.Util;

namespace Hanekawa.Modules.Audio
{
    public class Music : InteractiveBase
    {
        private readonly AudioService _audioService;

        public Music(AudioService audioService)
        {
            _audioService = audioService;
        }

        [Command("summon", RunMode = RunMode.Async)]
        [Alias("connect", "join")]
        [Summary("Connects the bot to the voice channel the user is in.")]
        [RequiredChannel]
        public async Task SummonAudio()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            var ch = (Context.User as IVoiceState)?.VoiceChannel;
            await _audioService.Summon(Context.Guild, ch);
            await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"Connected to {ch?.Name}").Build(),
                TimeSpan.FromSeconds(15));
            await Context.Message.DeleteAsync();
        }

        [Command("reconnect", RunMode = RunMode.Async)]
        [Summary("Forces the bot to disconnect and reconnect to the channel(incase of hanging up)")]
        [RequiredChannel]
        public async Task Reconnect()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            var ch = (Context.User as IVoiceState).VoiceChannel;
            await _audioService.Reconnect(Context.Guild, ch);
            await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply("Reconnected").Build(),
                TimeSpan.FromSeconds(15));
            await Context.Message.DeleteAsync();
        }

        [Command("start", RunMode = RunMode.Async)]
        [Summary("Starts the player")]
        [RequiredChannel]
        public async Task StartAudio()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            var ch = (Context.User as IVoiceState).VoiceChannel;
            var response = await _audioService.Start(Context.Guild, ch);
            await ReplyAndDeleteAsync(null, false, response.Build(), TimeSpan.FromSeconds(15));
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Summary("Pauses the player")]
        [RequiredChannel]
        public async Task PauseAudio()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            var ch = (Context.User as IVoiceState).VoiceChannel;
            var response = await _audioService.Pause(Context.Guild, ch);
            await Context.Message.DeleteAsync();
            await ReplyAndDeleteAsync(null, false, response.Build(), TimeSpan.FromSeconds(15));
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Plays a song and joins the channel the user is in")]
        [RequiredChannel]
        public async Task PlayAudio([Remainder] string query)
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            if (query.IsNullOrWhiteSpace())
            {
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().Reply($"{Context.User.Mention} please specify a song to play",
                            Color.Red.RawValue)
                        .Build(), TimeSpan.FromSeconds(15));
                return;
            }

            try
            {
                var ch = (Context.User as IVoiceState).VoiceChannel;
                var embed = await _audioService.Play(Context.Guild.Id, ch, query);
                await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(15));
            }
            catch
            {
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().Reply($"{Context.User.Mention}, I couldn't play that", Color.Red.RawValue)
                        .Build(), TimeSpan.FromSeconds(15));
            }
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Stops the player")]
        [RequiredChannel]
        public async Task StopAudio()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            await _audioService.Stop(Context.Guild);
            await Context.Message.DeleteAsync();
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"{Context.User.Mention} Stopped player").Build(), TimeSpan.FromSeconds(15));
        }

        [Command("destroy", RunMode = RunMode.Async)]
        [Summary("Destroys the player and disconnects the bot")]
        [RequiredChannel]
        public async Task DestroyAudio()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            await _audioService.Destroy(Context.Guild);
            await Context.Message.DeleteAsync();
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"{Context.User.Mention} Destroyed player").Build(), TimeSpan.FromSeconds(15));
        }

        [Command("volume", RunMode = RunMode.Async)]
        [Summary("Sets volume of the bot")]
        [RequiredChannel]
        public async Task VolumeAudio(uint volume)
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            if (volume > 100) volume = 100;
            if (volume < 1) volume = 1;
            await Context.Message.DeleteAsync();
            await _audioService.SetVolume(Context.Guild, volume);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"{Context.User.Mention} set volume to {volume}%").Build(),
                TimeSpan.FromSeconds(15));
        }

        [Command("playlist", RunMode = RunMode.Async)]
        [Summary("Adds a playlist to the queue")]
        [RequiredChannel]
        public async Task AddPlaylist(string playlist)
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            var ch = (Context.User as IVoiceState).VoiceChannel;
            await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply("Adding songs...").Build(),
                TimeSpan.FromSeconds(15));
            await Task.Delay(1000);
            await Context.Message.DeleteAsync();
            var aaaaaaaaaaaaaaaaaaaaaaaaaaah =
                await _audioService.AddPlaylistToQueueAsync(playlist, Context.User as IGuildUser, ch);
            await ReplyAsync($"Added {aaaaaaaaaaaaaaaaaaaaaaaaaaah} songs to the queue");
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Summary("Displays the current queue")]
        [RequiredChannel]
        public async Task GetQueue()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            try
            {
                var queue = _audioService.GetQueue(Context.Guild.Id).ToList();
                var player = _audioService.GetCurrentPlayer(Context.Guild);
                if (queue.Count == 0 && player.CurrentTrack != null)
                {
                    var author = new EmbedAuthorBuilder
                    {
                        IconUrl = "https://i.imgur.com/DIi4O65.png",
                        Name = $"Current song: {player.CurrentTrack.Title}",
                        Url = player.CurrentTrack.Url
                    };
                    var embed = new EmbedBuilder
                    {
                        Description = "Queue is empty",
                        Color = Color.Purple,
                        Author = author
                    };
                    await ReplyAsync(null, false, embed.Build());
                }

                var pages = new List<string>();
                for (var i = 0; i < queue.Count;)
                {
                    string song = null;
                    for (var j = 0; j < 10; j++)
                    {
                        if (i == queue.ToList().Count) continue;
                        var context = queue[i];
                        song += $"{i + 1} - {context.Title} by {context.Author}\n";
                        i++;
                    }

                    pages.Add(song);
                }

                var editPaginater = new PaginatedMessage
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = "https://i.imgur.com/DIi4O65.png",
                        Name = $"Current song: {_audioService.GetCurrentTrack(Context.Guild).Title}",
                        Url = $"{_audioService.GetCurrentTrack(Context.Guild).Url}"
                    },
                    Pages = pages,
                    Color = Color.Purple
                };
                await PagedReplyAsync(editPaginater);
            }
            catch
            {
                await ReplyAsync(null, false, new EmbedBuilder().Reply("Currently not playing").Build());
            }
        }

        [Command("current", RunMode = RunMode.Async)]
        [Alias("cr")]
        [Summary("Displays current playing song")]
        [RequiredChannel]
        public async Task CurrentSong()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            var embed = _audioService.GetCurrentSong(Context.Guild);
            if (embed == null)
                await ReplyAsync(null, false, new EmbedBuilder().Reply("Currently not playing.").Build());
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Clears queue")]
        [RequiredChannel]
        public async Task ClearQueue()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            _audioService.ClearQueue(Context.Guild.Id);
            await Context.Message.DeleteAsync();
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"{Context.User.Mention} Cleared queue.").Build(), TimeSpan.FromSeconds(15));
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skips song")]
        [RequiredChannel]
        public async Task SkipSong()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            var ch = (Context.User as IVoiceState)?.VoiceChannel;
            await _audioService.SkipSong(Context.Guild, ch);
            await Context.Message.DeleteAsync();
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"{Context.User.Mention} Skipped song.").Build(), TimeSpan.FromSeconds(15));
        }

        [Command("loop", RunMode = RunMode.Async)]
        [Summary("Loops the queue")]
        [RequiredChannel]
        public async Task LoopSongs()
        {
            if (Context.User as IVoiceState == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            await ReplyAsync(null, false, _audioService.ToggleLoop(Context.Guild).Build());
        }
    }
}