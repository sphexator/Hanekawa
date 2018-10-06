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
        //private readonly PlaylistService _playlist;

        public Music(AudioService audioService)
        {
            _audio = audioService;
            //_playlist = playlist;
        }

        [Command("Join", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task Join()
            => await _audio.ConnectAsync(Context.Guild.Id, Context.User as IGuildUser, Context.Channel);

        [Command("Leave", RunMode = RunMode.Async), Alias("Stop")]
        [RequiredChannel]
        public async Task StopAsync()
            => await ReplyAsync(null, false, (await _audio.StopAsync(Context.Guild.Id)).Build());

        [Command("Leave", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task Leave()
            => await ReplyAsync(null, false, (await _audio.DisconnectAsync(Context.Guild.Id)).Build());

        [Command("Play", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task PlayAsync([Remainder] string query)
            => await ReplyAsync(null, false, (await _audio.PlayAsync(Context.User as IGuildUser, query, Context.User as IGuildUser, Context.Channel)).Build());

        [Command("Pause", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task Pause()
            => await ReplyAsync(null, false, _audio.Pause(Context.Guild.Id).Build());

        [Command("Resume", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task Resume()
            => await ReplyAsync(null, false, _audio.Resume(Context.Guild.Id).Build());

        [Command("Queue", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task Queue()
            => await ReplyAsync(null, false, _audio.DisplayQueue(Context.Guild.Id).Build());

        [Command("Seek", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task Seek(TimeSpan span)
            => await ReplyAsync(null, false, _audio.Seek(Context.Guild.Id, span).Build());

        [Command("Skip", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task SkipAsync()
            => await ReplyAsync(null, false, (await _audio.SkipAsync(Context.Guild.Id, Context.User.Id)).Build());

        [Command("Volume", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task Volume(int volume)
            => await ReplyAsync(null, false, _audio.Volume(Context.Guild.Id, volume).Build());

        [Command("repeat", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task Repeat()
            => await ReplyAsync(null, false, _audio.Repeat(Context.Guild.Id).Build());

        [Command("audio fix", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task FixPlayer()
            => await ReplyAsync(null, false,
                (await _audio.FixPlayer(Context.Guild.Id, Context.User as IGuildUser, Context.Channel)).Build());
    }
}