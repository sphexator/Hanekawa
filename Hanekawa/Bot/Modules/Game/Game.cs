using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Game.Ship;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Game
{
    [Name("Ship Game")]
    [Description(
        "Ship game is a game mode where you search for opponements based on your level and fight them. Change between classes to get a feel of different fight styles.")]
    [RequiredChannel]
    public class Game : HanekawaModule
    {
        private readonly ShipGameService _shipGame;
        public Game(ShipGameService shipGame) => _shipGame = shipGame;

        [Name("Search")]
        [Command("search")]
        [Description("Searches for a monster to fight")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        public async Task SearchAsync()
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var embed = await _shipGame.SearchAsync(Context);
            if (embed == null) return;
            await Context.ReplyAsync(embed);
        }

        [Name("Attack")]
        [Command("attack")]
        [Description("Starts a fight with a monster you've found")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        public async Task AttackAsync() => await _shipGame.AttackAsync(Context);

        [Name("Duel")]
        [Command("duel")]
        [Description("Duels a user. Add an amount to duel for credit")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        public async Task DuelAsync(CachedMember user, int? bet = null) =>
            await _shipGame.AttackAsync(Context, user, bet);

        [Name("Class Info")]
        [Command("classinfo")]
        [Description("Display all classes in a paginated message")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        public async Task ClassInfoAsync()
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var result = new List<string>();
            var classes = await db.GameClasses.OrderBy(x =>  x.LevelRequirement).ToListAsync();
            for (var i = 0; i < classes.Count(); i++)
            {
                var x = classes[i];
                result.Add($"{x.Id} - {x.Name} (level: {x.LevelRequirement})");
            }

            if (result.Count == 0) await Context.ReplyAsync("Something went wrong...\nCouldn't get a list of classes.");
            else await Context.PaginatedReply(result, Context.Guild, "Game Classes");
        }

        [Name("Class Info")]
        [Command("classinfo")]
        [Description("Display information on a specific class providing ID")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        public async Task ClassInfoAsync(int classId)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var classInfo = await db.GameClasses.FindAsync(classId);
            if (classInfo == null)
            {
                await Context.ReplyAsync("Couldn't find a class with that ID", Color.Red);
                return;
            }

            await Context.ReplyAsync($"Information for **{classInfo.Name}**\n" +
                                     $"Health: {100 * classInfo.ModifierHealth}\n" +
                                     $"Damage: {100 * classInfo.ModifierDamage}\n" +
                                     $"Critical Chance: {classInfo.ChanceCrit}\n" +
                                     $"Avoidance: {classInfo.ChanceAvoid}");
        }

        [Name("Choose Class")]
        [Command("class")]
        [Description("Choose or change into a class with its ID")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        public async Task ChooseClassAsync(int id)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var classInfo = await db.GameClasses.FindAsync(id);
            var userData = await db.GetOrCreateUserData(Context.Member);
            if (userData.Level < (int) classInfo.LevelRequirement)
            {
                await Context.ReplyAsync("Not high enough level for this class yet");
                return;
            }

            userData.Class = classInfo.Id;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Switched class to **{classInfo.Name}**", Color.Green);
        }
    }
}