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

        public Music(AudioService audioService) => _audio = audioService;

        [Name("Join")]
        [Command("join", RunMode = RunMode.Async)]
        [Alias("summon", "connect")]
        [Summary("Joins the voice channel the user is in")]
        [Remarks("h.join")]
        public async Task Join() =>
            await _audio.ConnectAsync(Context.Guild.Id, Context.User as IGuildUser, Context.Channel);
        
        [Name("Stop")]
        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Leaves and destroys the player")]
        [Remarks("h.stop")]
        public async Task StopAsync() => await Context.ReplyAsync(await _audio.StopAsync(Context.Guild.Id));

        [Name("h.leave")]
        [Command("Leave", RunMode = RunMode.Async)]
        [Alias("disconnect")]
        [Summary("Leaves voice channel")]
        [Remarks("h.leave")]
        public async Task Leave() => await Context.ReplyAsync(await _audio.DisconnectAsync(Context.Guild.Id));

        [Name("Play")]
        [Command("Play", RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("Plays or queues a song")]
        [Remarks("h.p https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
        public async Task PlayAsync([Remainder] string query) =>
            await Context.ReplyAsync(
                await _audio.PlayAsync(Context.Guild.Id, query, Context.Channel as ITextChannel,
                    Context.User as IGuildUser));

        [Name("Pause")]
        [Command("Pause", RunMode = RunMode.Async)]
        [Summary("Pauses the player")]
        [Remarks("h.pause")]
        public async Task Pause() => await Context.ReplyAsync(await _audio.PauseAsync(Context.Guild.Id));

        [Name("Loop")]
        [Command("loop")]
        [Summary("Loops current playlist")]
        [Remarks("h.loop")]
        public async Task Loop() => await Context.ReplyAsync(_audio.LoopPlayer(Context.Guild.Id));

        [Name("Repeat")]
        [Command("repeat")]
        [Summary("Repeats track currently playing")]
        [Remarks("h.repeat")]
        public async Task Repeat() => await Context.ReplyAsync(_audio.Repeat(Context.Guild.Id));

        [Name("Resume")]
        [Command("Resume", RunMode = RunMode.Async)]
        [Summary("Resumes the player")]
        [Remarks("h.resume")]
        public async Task Resume() => await Context.ReplyAsync(await _audio.ResumeAsync(Context.Guild.Id));

        [Name("Queue")]
        [Command("Queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("Displays current queue")]
        [Remarks("h.queue")]
        public async Task Queue() => await Context.ReplyAsync(_audio.DisplayQueue(Context.Guild.Id));

        [Name("Clear")]
        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Clears the queue")]
        [Remarks("h.clear")]
        public async Task ClearQueue() => await Context.ReplyAsync(await _audio.ClearQueueAsync(Context.Guild.Id));

        [Name("Seek")]
        [Command("Seek", RunMode = RunMode.Async)]
        [Summary("Jumps to a certain part in the song/video")]
        [Remarks("h.seek 1m30s")]
        public async Task Seek(TimeSpan span) =>
            await Context.ReplyAsync(await _audio.SeekAsync(Context.Guild.Id, span));

        [Name("Skip")]
        [Command("Skip", RunMode = RunMode.Async)]
        [Alias("next")]
        [Summary("Skips song")]
        [Remarks("h.next")]
        public async Task SkipAsync() =>
            await Context.ReplyAsync(await _audio.SkipAsync(Context.Guild.Id, Context.User as SocketGuildUser));

        [Name("Volume")]
        [Command("Volume", RunMode = RunMode.Async)]
        [Summary("Sets volume of song, based on the volume of the video/source")]
        [Remarks("h.volume 50")]
        public async Task Volume(int volume) =>
            await ReplyAsync(null, false, (await _audio.VolumeAsync(Context.Guild.Id, volume)).Build());

        [Name("Audio fix")]
        [Command("audio fix", RunMode = RunMode.Async)]
        [Summary("In case the player bugs out or music isn't playing, use this to attempt to fix it")]
        [Remarks("h.audio fix")]
        public async Task FixPlayer() =>
            await Context.ReplyAsync(await _audio.FixPlayer(Context.Guild.Id, Context.User as IGuildUser,
                Context.Channel));
    }
}