using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions.Embed;
using Hanekawa.Modules.Audio.Service;
using Hanekawa.Preconditions;

namespace Hanekawa.Modules.Audio
{
    [RequireContext(ContextType.Guild)]
    [RequiredChannel]
    public class Music : InteractiveBase
    {
        private readonly AudioService _audio;

        public Music(AudioService audioService)
        {
            _audio = audioService;
        }

        [Command("Join", RunMode = RunMode.Async)]
        [Alias("summon", "connect")]
        [Summary("Joins the voice channel the user is in")]
        public async Task Join() =>
            await _audio.ConnectAsync(Context.Guild.Id, Context.User as IGuildUser, Context.Channel);

        [Command("Leave", RunMode = RunMode.Async)]
        [Alias("Stop")]
        [Summary("Leaves and destroys the player")]
        public async Task StopAsync() => await Context.ReplyAsync(await _audio.StopAsync(Context.Guild.Id));

        [Command("Leave", RunMode = RunMode.Async)]
        [Summary("Leaves voice channel")]
        public async Task Leave() => await Context.ReplyAsync(await _audio.DisconnectAsync(Context.Guild.Id));

        [Command("Play", RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("Plays or queues a song")]
        public async Task PlayAsync([Remainder] string query) => await Context.ReplyAsync(
            await _audio.PlayAsync(Context.Guild.Id, query, Context.Channel as ITextChannel,
                Context.User as IGuildUser));

        [Command("Pause", RunMode = RunMode.Async)]
        [Summary("Pauses the player")]
        public async Task Pause() => await Context.ReplyAsync(await _audio.PauseAsync(Context.Guild.Id));

        [Command("loop")]
        [Summary("Loops current playlist")]
        public async Task Loop() => await Context.ReplyAsync(_audio.LoopPlayer(Context.Guild.Id));

        [Command("repeat")]
        [Summary("Repeats track currently playing")]
        public async Task Repeat() => await Context.ReplyAsync(_audio.Repeat(Context.Guild.Id));

        [Command("Resume", RunMode = RunMode.Async)]
        [Summary("Resumes the player")]
        public async Task Resume() => await Context.ReplyAsync(await _audio.ResumeAsync(Context.Guild.Id));

        [Command("Queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("Displays current queue")]
        public async Task Queue() => await Context.ReplyAsync(_audio.DisplayQueue(Context.Guild.Id));

        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Clears the queue")]
        public async Task ClearQueue() => await Context.ReplyAsync(await _audio.ClearQueueAsync(Context.Guild.Id));

        [Command("Seek", RunMode = RunMode.Async)]
        [Summary("Jumps to a certain part in the song/video")]
        public async Task Seek(TimeSpan span) =>
            await Context.ReplyAsync(await _audio.SeekAsync(Context.Guild.Id, span));

        [Command("Skip", RunMode = RunMode.Async)]
        [Alias("next")]
        [Summary("Skips song")]
        public async Task SkipAsync() =>
            await Context.ReplyAsync(await _audio.SkipAsync(Context.Guild.Id, Context.User as SocketGuildUser));

        [Command("Volume", RunMode = RunMode.Async)]
        [Summary("Sets volume of song, based on the volume of the video/source")]
        public async Task Volume(int volume) =>
            await ReplyAsync(null, false, (await _audio.VolumeAsync(Context.Guild.Id, volume)).Build());

        [Command("audio fix", RunMode = RunMode.Async)]
        [Summary("Incase the player bugs out or music isn't playing, use this to attempt to fix it")]
        public async Task FixPlayer() =>
            await Context.ReplyAsync(await _audio.FixPlayer(Context.Guild.Id, Context.User as IGuildUser,
                Context.Channel));
    }
}