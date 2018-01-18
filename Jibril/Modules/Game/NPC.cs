using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Modules.Game.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Game
{
    public class NPC : ModuleBase<SocketCommandContext>
    {
        [Command("search", RunMode = RunMode.Async)]
        [Alias("find", "radar")]
        [RequiredChannel(346429281314013184)]
        public async Task FindNPC()
        {
            try
            {
                var user = Context.User;
                var userData = DatabaseService.UserData(user).FirstOrDefault();
                var result = GameDatabase.GameCheckExistingUser(user);
                if (result.Count <= 0)
                {
                    var userHealth = BaseStats.HealthPoint(userData.Level, userData.ShipClass);
                    GameDatabase.AddNPCDefault(user, userHealth);
                }

                var GameStatus = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
                if (GameStatus.Combatstatus == 1)
                {
                    var embed = CombatResponse.Combat(user, Colours.OkColour, GameStatus);
                    await ReplyAsync($"{user.Username}, You're already in a fight, use !attack to fight the enemy",
                        false, embed.Build());
                }
                else
                {
                    var embed = FindEnemy.FindEnemyNPC(user, userData);
                    await ReplyAsync("", false, embed.Build());
                }
            }
            catch
            {
            }
        }

        [Command("attack", RunMode = RunMode.Async)]
        [Ratelimit(1, 5, Measure.Seconds, false, false)]
        [RequiredChannel(346429281314013184)]
        public async Task AttackTarget()
        {
            var user = Context.User;
            var gameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
            if (gameData.Combatstatus == 1)
            {
                var userData = DatabaseService.UserData(user).FirstOrDefault();
                var enemyData = GameDatabase.Enemy(gameData.Enemyid).FirstOrDefault();
                var embed = Combat.CombatDamage(user, gameData, userData, enemyData);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} is currently not in a fight.",
                    Colours.DefaultColour);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("health")]
        [Alias("hp")]
        [RequiredChannel(346429281314013184)]
        public async Task SelfHealth()
        {
            var user = Context.User;
            var gamedata = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
            var health = gamedata.Health - gamedata.Damagetaken;
            var embed = EmbedGenerator.AuthorEmbed($"Health: {health}/{gamedata.Health}", $"{user.Mention}",
                Colours.DefaultColour, user);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("flee")]
        [Alias("run")]
        [RequiredChannel(346429281314013184)]
        public async Task FleeFromCombat()
        {
            var user = Context.User;
            try
            {
                GameDatabase.FinishedNPCFight(user);
                var embed = EmbedGenerator.DefaultEmbed($"{user.Username} has fleed from combat",
                    Colours.DefaultColour);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
            catch
            {
                await ReplyAsync(":thinking:").ConfigureAwait(false);
            }
        }
    }
}