using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Models.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Controllers
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
        public async Task<Leaderboard> GetLeaderboardAsync([FromBody] string rawId)
        {
            if (!ulong.TryParse(rawId, out var id)) return null;
            var guild = _bot.GetGuild(id);
            if (guild == null) return null;
            var toReturn = new Leaderboard();
            var users = await _db.Accounts.Where(x => x.GuildId == id && x.Active).Take(100)
                .OrderByDescending(x => x.TotalExp - x.Decay).ToListAsync();
            var exp = _provider.GetRequiredService<ExpService>();
            foreach (var x in users)
            {
                var user = guild.GetMember(x.UserId);
                if (user != null)
                {
                    toReturn.Users.Add(new LeaderboardUser
                    {
                        UserId = x.UserId,
                        ExpToLevel = exp.ExpToNextLevel(x.Level),
                        Experience = x.Exp
                    });
                }
            }
            return toReturn;
        }

        [HttpGet("{rawId}/weekly")]
        public async Task<LeaderboardWeekly> GetWeeklyLeaderboardAsync([FromBody] string rawId)
        {
            if (!ulong.TryParse(rawId, out var id)) return null;
            var guild = _bot.GetGuild(id);
            if (guild == null) return null;
            var toReturn = new LeaderboardWeekly();
            var users = await _db.Accounts.Where(x => x.GuildId == id && x.Active).Take(100)
                .OrderByDescending(x => x.MvpCount).ToListAsync();
            foreach (var x in users)
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
        public async Task<LeaderboardWeekly> GetRichestLeaderboardAsync([FromBody] string rawId)
        {
            if (!ulong.TryParse(rawId, out var id)) return null;
            var guild = _bot.GetGuild(id);
            if (guild == null) return null;
            var toReturn = new LeaderboardWeekly();
            var users = await _db.Accounts.Where(x => x.GuildId == id && x.Active).Take(100)
                .OrderByDescending(x => x.Credit).ToListAsync();
            foreach (var x in users)
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
