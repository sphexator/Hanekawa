using System;
using System.Collections.Concurrent;
using System.Threading;
using Discord.WebSocket;

namespace Jibril.Modules.Marriage.Service
{
    public class MarriageService
    {
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> WaifuTimer { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();

        private readonly DiscordSocketClient _client;
        private const ulong GuildId = 339370914724446208;

        public MarriageService(DiscordSocketClient client)
        {
            _client = client;
            var waifus = MarriageDb.GetMarriageData();
            foreach (var x in waifus)
            {
                TimeSpan after;
                if (x.Timer - TimeSpan.FromMinutes(2) <= DateTime.UtcNow)
                {
                    after = TimeSpan.FromMinutes(2);
                }
                else
                {
                    after = x.Timer - DateTime.UtcNow;
                }
                StartWaifuTimer(x.Userid, after);
            }
        }

        private void StartWaifuTimer(ulong userId, TimeSpan after)
        {
            var waifuTimer = WaifuTimer.GetOrAdd(GuildId, new ConcurrentDictionary<ulong, Timer>());

            var toAdd = new Timer(async _ =>
            {
                try
                {

                }
                catch (Exception e)
                {
                    //Ignore
                }
            });

            waifuTimer.AddOrUpdate(userId, (key) => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }
    }
}
