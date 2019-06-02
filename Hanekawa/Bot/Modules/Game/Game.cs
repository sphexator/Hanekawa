using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Game.Ship;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Extensions.Embed;
using Qmmands;
using Cooldown = Hanekawa.Core.Cooldown;

namespace Hanekawa.Bot.Modules.Game
{
    public class Game : InteractiveBase
    {
        private readonly ShipGameService _shipGame;
        public Game(ShipGameService shipGame) => _shipGame = shipGame;

        [Name("Search")]
        [Command("search")]
        [Description("Searches for a monster to fight")]
        [Cooldown(1, 2, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task SearchAsync()
        {
            using var db = new DbService();
            await Context.ReplyAsync(await _shipGame.SearchAsync(Context.User, db));
        }

        [Name("Attack")]
        [Command("attack")]
        [Description("Starts a fight with a monster you've found")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task AttackAsync() { }

        [Name("Duel")]
        [Command("duel")]
        [Description("Duels a user. Add an amount to duel for credit")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task DuelAsync(SocketGuildUser user, int? bet = null) { }
    }
}