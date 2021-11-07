using System.Threading;
using System.Threading.Tasks;
using Disqord.Gateway;
using Hanekawa.Infrastructure;
using Hanekawa.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BanController : ControllerBase
    {
        private readonly Bot.Hanekawa _bot;
        private readonly DbService _db;

        public BanController(Bot.Hanekawa bot, DbService db)
        {
            _bot = bot;
            _db = db;
        }

        [HttpGet("{rawId}/{userId}")]
        public async Task<BanCase> GetBanCaseAsync([FromRoute] string rawId, [FromRoute]string userId, CancellationToken token)
        {
            if (!ulong.TryParse(rawId, out var guildId)) return null;
            var guild = _bot.GetGuild(guildId);
            if (guild == null) return null;
            if (!ulong.TryParse(userId, out var id)) return null;
            var modCase = await _db.ModLogs.FirstOrDefaultAsync(
                x => x.Action == "Ban" && x.GuildId == guildId && x.UserId == id, token);
            return new BanCase(modCase);
        }
    }
}