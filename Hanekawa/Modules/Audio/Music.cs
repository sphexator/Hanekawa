using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Modules.Audio.Service;
using System;
using System.Threading.Tasks;
using Hanekawa.Preconditions;

namespace Hanekawa.Modules.Audio
{
    [RequireContext(ContextType.Guild)]
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
        [RequiredChannel]
        public async Task Join()
            => await _audio.ConnectAsync(Context.Guild.Id, Context.User as IGuildUser, Context.Channel);

        [Command("Leave", RunMode = RunMode.Async), Alias("Stop")]
        [Summary("Leaves and destroys the player")]
        [RequiredChannel]
        public async Task StopAsync()
            => await ReplyAsync(null, false, (await _audio.StopAsync(Context.Guild.Id)).Build());

        [Command("Leave", RunMode = RunMode.Async)]
        [Summary("Leaves voice channel")]
        [RequiredChannel]
        public async Task Leave()
            => await ReplyAsync(null, false, (await _audio.DisconnectAsync(Context.Guild.Id)).Build());

        [Command("Play", RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("Plays or queues a song")]
        [RequiredChannel]
        public async Task PlayAsync([Remainder] string query)
            => await ReplyAsync(null, false, (await _audio.PlayAsync(Context.User as IGuildUser, query, Context.User as IGuildUser, Context.Channel as ITextChannel)).Build());

        [Command("Pause", RunMode = RunMode.Async)]
        [Summary("Pauses the player")]
        [RequiredChannel]
        public async Task Pause()
            => await ReplyAsync(null, false, (await _audio.PauseAsync(Context.Guild.Id)).Build());

        [Command("Resume", RunMode = RunMode.Async)]
        [Summary("Resumes the player")]
        [RequiredChannel]
        public async Task Resume()
            => await ReplyAsync(null, false, (await _audio.ResumeAsync(Context.Guild.Id)).Build());

        [Command("Queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("Displays current queue")]
        [RequiredChannel]
        public async Task Queue()
            => await ReplyAsync(null, false, _audio.DisplayQueue(Context.Guild.Id).Build());

        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Clears the queue")]
        [RequiredChannel]
        public async Task ClearQueue()
            => await ReplyAsync(null, false, (await _audio.ClearQueueAsync(Context.Guild.Id)).Build());


        [Command("Seek", RunMode = RunMode.Async)]
        [Summary("Jumps to a certain part in the song/video")]
        [RequiredChannel]
        public async Task Seek(TimeSpan span)
            => await ReplyAsync(null, false, (await _audio.SeekAsync(Context.Guild.Id, span)).Build());

        [Command("Skip", RunMode = RunMode.Async)]
        [Alias("next")]
        [Summary("Skips song")]
        [RequiredChannel]
        public async Task SkipAsync()
            => await ReplyAsync(null, false, (await _audio.SkipAsync(Context.Guild.Id, Context.User.Id)).Build());

        [Command("Volume", RunMode = RunMode.Async)]
        [Summary("Sets volume of song, based on the volume of the video/source")]
        [RequiredChannel]
        public async Task Volume(int volume)
            => await ReplyAsync(null, false, (await _audio.VolumeAsync(Context.Guild.Id, volume)).Build());

        [Command("repeat", RunMode = RunMode.Async)]
        [Summary("Repeats the queue")]
        [RequiredChannel]
        public async Task Repeat()
            => await ReplyAsync(null, false, _audio.Repeat(Context.Guild.Id).Build());

        [Command("audio fix", RunMode = RunMode.Async)]
        [Summary("Incase the player bugs out or music isn't playing, use this to attempt to fix it")]
        [RequiredChannel]
        public async Task FixPlayer()
            => await ReplyAsync(null, false,
                (await _audio.FixPlayer(Context.Guild.Id, Context.User as IGuildUser, Context.Channel)).Build());
    }
}