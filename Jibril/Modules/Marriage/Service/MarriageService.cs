using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

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

        public void AddWaifu(IGuildUser user, IGuildUser claimUser)
        {
            var after = TimeSpan.FromDays(7);
            var waifuUpgradeAt = DateTime.UtcNow + after;
            MarriageDb.EnterUser(user, claimUser, waifuUpgradeAt);
            MarriageDb.EnterUser(claimUser, user, waifuUpgradeAt);
            StartWaifuTimer(user.Id, after);
            StartWaifuTimer(claimUser.Id, after);
        }

        public void RemoveWaifu(IGuildUser user)
        {
            RemoveWaifuTimer(user.Id);
            var waifu = MarriageDb.MarriageData(user.Id).FirstOrDefault();
            RemoveWaifuFromDb(user.Id, waifu.Claim);
        }

        private void StartWaifuTimer(ulong userId, TimeSpan after)
        {
            var waifuTimer = WaifuTimer.GetOrAdd(GuildId, new ConcurrentDictionary<ulong, Timer>());

            var toAdd = new Timer(_ =>
            {
                var user = MarriageDb.MarriageData(userId).FirstOrDefault();
                try
                {
                    if (user != null && user.Rank == 2)
                    {

                    }
                }
                catch (Exception)
                {
                    //Ignore
                }
            }, null, after, Timeout.InfiniteTimeSpan);

            waifuTimer.AddOrUpdate(userId, (key) => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }

        private void RemoveWaifuTimer(ulong userid)
        {
            if (!WaifuTimer.TryGetValue(GuildId, out ConcurrentDictionary<ulong, Timer> waifu)) return;

            if (waifu.TryRemove(userid, out Timer removed))
            {
                removed.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void RemoveWaifuFromDb(ulong user, ulong claimUser)
        {
            MarriageDb.RemoveWaifu(user, claimUser);
        }
    }
}
