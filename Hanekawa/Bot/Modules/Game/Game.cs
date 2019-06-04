using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Game.Ship;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Extensions.Embed;
using Qmmands;
using Cooldown = Hanekawa.Core.Cooldown;

namespace Hanekawa.Bot.Modules.Game
{
    [Name("Ship Game")]
    [Description("Ship game is a game mode where you search for opponements based on your level and fight them. Change between classes to get a feel of different fight styles.")]
    [RequiredChannel]
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
        public async Task AttackAsync() => await _shipGame.PvEBattle(Context);

        [Name("Duel")]
        [Command("duel")]
        [Description("Duels a user. Add an amount to duel for credit")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task DuelAsync(SocketGuildUser user, int? bet = null) =>
            await _shipGame.PvPBattle(Context, user, bet);

        [Name("Class Info")]
        [Command("class info")]
        [Description("")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task ClassInfoAsync()
        {

        }

        [Name("Class Info")]
        [Command("class info")]
        [Description("")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task ClassInfoAsync(int classId)
        {

        }

        [Name("Choose Class")]
        [Command("class")]
        [Description("Choose or change into a class with its ID")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task ChooseClassAsync()
        {

        }
    }
}