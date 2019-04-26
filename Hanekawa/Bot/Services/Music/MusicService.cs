using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Victoria;

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

            _lavaClient.Log += _lavaClient_Log;
            _lavaClient.OnPlayerUpdated += _lavaClient_OnPlayerUpdated;
            _lavaClient.OnServerStats += _lavaClient_OnServerStats;
            _lavaClient.OnSocketClosed += _lavaClient_OnSocketClosed;
            _lavaClient.OnTrackException += _lavaClient_OnTrackException;
            _lavaClient.OnTrackFinished += _lavaClient_OnTrackFinished;
            _lavaClient.OnTrackStuck += _lavaClient_OnTrackStuck;
        }

        private Task _lavaClient_OnTrackStuck(LavaPlayer arg1, Victoria.Entities.LavaTrack arg2, long arg3)
        {
            throw new NotImplementedException();
        }

        private Task _lavaClient_OnTrackFinished(LavaPlayer arg1, Victoria.Entities.LavaTrack arg2, Victoria.Entities.TrackEndReason arg3)
        {
            throw new NotImplementedException();
        }

        private Task _lavaClient_OnTrackException(LavaPlayer arg1, Victoria.Entities.LavaTrack arg2, string arg3)
        {
            throw new NotImplementedException();
        }

        private Task _lavaClient_OnSocketClosed(int arg1, string arg2, bool arg3)
        {
            throw new NotImplementedException();
        }

        private Task _lavaClient_OnServerStats(Victoria.Entities.ServerStats arg)
        {
            throw new NotImplementedException();
        }

        private Task _lavaClient_OnPlayerUpdated(LavaPlayer arg1, Victoria.Entities.LavaTrack arg2, TimeSpan arg3)
        {
            throw new NotImplementedException();
        }

        private Task _lavaClient_Log(Discord.LogMessage arg)
        {
            throw new NotImplementedException();
        }
    }
}
