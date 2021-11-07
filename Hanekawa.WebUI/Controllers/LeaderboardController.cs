using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Disqord.Gateway;
using Hanekawa.Infrastructure;
using Hanekawa.WebUI.Extensions;
using Hanekawa.WebUI.Bot.Service.Experience;
using Hanekawa.WebUI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly Bot.Hanekawa _bot;
        private readonly IServiceProvider _provider;
        private readonly DbService _db;

        public LeaderboardController(Bot.Hanekawa bot, IServiceProvider provider, DbService db)
        {
            _bot = bot;
            _provider = provider;
            _db = db;
        }

        [HttpGet("{rawId}")]
        public async Task<Leaderboard> GetLeaderboardAsync([FromRoute] string rawId, CancellationToken token)
        {
            if (!ulong.TryParse(rawId, out var id)) return null;
            var guild = _bot.GetGuild(id);
            if (guild == null) return null;
            var toReturn = new Leaderboard {Users = new List<LeaderboardUser>()};
            var users = await _db.Accounts.Where(x => x.GuildId == id && x.Active)
                .OrderByDescending(x => x.TotalExp - x.Decay).Take(100).ToListAsync(cancellationToken: token);
            var exp = _provider.GetRequiredService<ExpService>();
            foreach (var x in users)
            {
                var user = await _bot.GetOrFetchMemberAsync(id, x.UserId);
                if (user != null)
                {
                    toReturn.Users.Add(new LeaderboardUser
                    {
                        UserId = x.UserId,
                        ExpToLevel = exp.ExpToNextLevel(x.Level),
                        Experience = x.Exp,
                        Level = x.Level,
                        TotalExp = x.TotalExp
                    });
                }
            }
            return toReturn;
        }

        [HttpGet("{rawId}/weekly")]
        public async Task<LeaderboardWeekly> GetWeeklyLeaderboardAsync([FromRoute] string rawId, CancellationToken token)
        {
            if (!ulong.TryParse(rawId, out var id)) return null;
            var guild = _bot.GetGuild(id);
            if (guild == null) return null;
            var toReturn = new LeaderboardWeekly {Users = new List<LeaderboardWeeklyUser>()};
            var users = await _db.Accounts.Where(x => x.GuildId == id && x.Active)
                .OrderByDescending(x => x.MvpCount).Take(100).ToListAsync(cancellationToken: token);
            foreach (var x in from x in users let user = _bot.GetMember(id, x.UserId) where user != null select x)
            {
                toReturn.Users.Add(new LeaderboardWeeklyUser
                {
                    UserId = x.UserId,
                    Points = x.MvpCount
                });
            }
            return toReturn;
        }

        [HttpGet("{rawId}/richest")]
        public async Task<LeaderboardWeekly> GetRichestLeaderboardAsync([FromRoute] string rawId, CancellationToken token)
        {
            if (!ulong.TryParse(rawId, out var id)) return null;
            var guild = _bot.GetGuild(id);
            if (guild == null) return null;
            var toReturn = new LeaderboardWeekly {Users = new List<LeaderboardWeeklyUser>()};
            var users = await _db.Accounts.Where(x => x.GuildId == id && x.Active)
                .OrderByDescending(x => x.Credit).Take(100).ToListAsync(cancellationToken: token);

            foreach (var x in from x in users let user = _bot.GetMember(id, x.UserId) where user != null select x)
            {
                toReturn.Users.Add(new LeaderboardWeeklyUser
                {
                    UserId = x.UserId,
                    Points = x.Credit
                });
            }
            return toReturn;
        }
    }
}
