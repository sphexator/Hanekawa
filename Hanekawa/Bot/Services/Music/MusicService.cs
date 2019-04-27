using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Extensions.Embed;
using Victoria;
using Victoria.Entities;

namespace Hanekawa.Bot.Services.Music
{
    public partial class MusicService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;
        private readonly Random _random;
        private readonly LavaSocketClient _lavaClient;

        public MusicService(DiscordSocketClient client, DbService db, Random random, LavaSocketClient lavaClient)
        {
            _client = client;
            _db = db;
            _random = random;
            _lavaClient = lavaClient;

            _lavaClient.OnTrackFinished += OnTrackFinished;
            _lavaClient.OnTrackException += OnTrackException;
            _lavaClient.OnTrackStuck += OnTrackStuck;
        }

        private Task OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            _ = Task.Run(async () =>
            {
                if (!reason.ShouldPlayNext()) return;

                if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
                {
                    await player.TextChannel.ReplyAsync("There are no more items left in queue.",
                        player.TextChannel.GuildId);
                    return;
                }

                await player.PlayAsync(nextTrack);
                await player.TextChannel.ReplyAsync($"Finished playing: {track.Title}\nNow playing: {nextTrack.Title}",
                    player.TextChannel.GuildId);
            });
            return Task.CompletedTask;
        }

        private Task OnTrackException(LavaPlayer player, LavaTrack track, string reason)
        {
            _ = Task.Run(async () => { });
            return Task.CompletedTask;
        }

        private Task OnTrackStuck(LavaPlayer player, LavaTrack track, long position)
        {
            _ = Task.Run(async () => { });
            return Task.CompletedTask;
        }
    }
}