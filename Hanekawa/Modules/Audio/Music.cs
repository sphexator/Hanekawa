using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database.Tables.Audio;
using Hanekawa.Modules.Audio.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Audio
{
    public class Music : InteractiveBase
    {
        private readonly AudioService _audio;
        private readonly PlaylistService _playlist;

        public Music(AudioService audioService, PlaylistService playlist)
        {
            _audio = audioService;
            _playlist = playlist;
        }

        [Command("Join", RunMode = RunMode.Async)]
        public async Task Join()
            => await _audio.ConnectAsync(Context.Guild.Id, Context.User as IGuildUser, Context.Channel);

        [Command("Leave", RunMode = RunMode.Async), Alias("Stop")]
        public async Task StopAsync()
            => await ReplyAsync(await _audio.StopAsync(Context.Guild.Id));

        [Command("Leave", RunMode = RunMode.Async)]
        public async Task Leave()
            => await ReplyAsync(await _audio.DisconnectAsync(Context.Guild.Id));

        [Command("Play", RunMode = RunMode.Async)]
        public async Task PlayAsync([Remainder] string query)
            => await ReplyAsync(await _audio.PlayAsync(Context.Guild.Id, query));

        [Command("Pause", RunMode = RunMode.Async)]
        public async Task Pause()
            => await ReplyAsync(_audio.Pause(Context.Guild.Id));

        [Command("Resume", RunMode = RunMode.Async)]
        public async Task Resume()
            => await ReplyAsync(_audio.Resume(Context.Guild.Id));

        [Command("Queue", RunMode = RunMode.Async)]
        public async Task Queue()
            => await ReplyAsync(_audio.DisplayQueue(Context.Guild.Id));

        [Command("Seek", RunMode = RunMode.Async)]
        public async Task Seek(TimeSpan span)
            => await ReplyAsync(_audio.Seek(Context.Guild.Id, span));

        [Command("Skip", RunMode = RunMode.Async)]
        public async Task SkipAsync()
            => await ReplyAsync(await _audio.SkipAsync(Context.Guild.Id, Context.User.Id));

        [Command("Volume", RunMode = RunMode.Async)]
        public async Task Volume(int volume)
            => await ReplyAsync(_audio.Volume(Context.Guild.Id, volume));

        [Command("playlist Create", RunMode = RunMode.Async), Alias("plNew")]
        public async Task CreateAsync(string name, bool isPrivate)
        {
            var playlist = new Playlist
            {
                Id = name,
                GuildId = Context.Guild.Id,
                IsPrivate = isPrivate,
                OwnerId = Context.User.Id,
                Tracks = new HashSet<string>()
            };

            var message = await _playlist.TryCreate(playlist);
            await ReplyAsync(message);
        }

        [Command("playlist Delete", RunMode = RunMode.Async), Alias("plRemove")]
        public async Task Delete(string name)
        {
            var message = await _playlist.TryDelete(name, Context.User.Id, Context.Guild.Id);
            await ReplyAsync(message);
        }

        [Command("playlist", RunMode = RunMode.Async), Alias("pl")]
        public async Task Info(string name)
        {
            if (_playlist.TryGet(name, Context.Guild.Id, out var playlist))
            {
                await ReplyAsync($"{name} is an unknown playlist.");
            }

            var message =
                "```diff" +
                $"- Name: {playlist.Id}      |      Owner: " +
                $"- Streams: {playlist.Streams}\n" +
                $"- Tracks: {playlist.Tracks.Count}\n" +
                $"- Playtime: {playlist.Playtime}";
            await ReplyAsync(message);
        }

        [Command("playlist Play", RunMode = RunMode.Async), Alias("plplay")]
        public async Task Play(string name)
        {
            var message = await _playlist.TryPlay(name, Context.Guild.Id, Context.User.Id, Context.Channel);
            await ReplyAsync(message);
        }
    }
}