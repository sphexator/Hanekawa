using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Extensions;
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
            using (var db = new hanekawaContext())
            {
                try
                {
                    var user = Context.User;
                    var userData = await db.GetOrCreateUserData(user);
                    var gameStatus = await db.GetOrCreateShipGame(user);
                    if (gameStatus.Combatstatus == 1)
                    {
                        var embed = await CombatResponse.Combat(user, Colours.OkColour, gameStatus);
                        await ReplyAsync($"{user.Username}, You're already in a fight, use !attack to fight the enemy",
                            false, embed.Build());
                    }
                    else
                    {
                        var embed = await FindEnemy.FindEnemyNPCAsync(user, userData);
                        await ReplyAsync("", false, embed.Build());
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        [Command("attack", RunMode = RunMode.Async)]
        [Ratelimit(1, 5, Measure.Seconds, false, false)]
        [RequiredChannel(346429281314013184)]
        public async Task AttackTarget()
        {
            using (var db = new hanekawaContext())
            {
                var user = Context.User;
                var gameData = await db.GetOrCreateShipGame(user);
                if (gameData.Combatstatus == 1)
                {
                    var userData = await db.GetOrCreateUserData(user);
                    var enemyData = await db.Enemyidentity.FindAsync(gameData.Enemyid.Value);
                    var embed = await Combat.CombatDamageAsync(user, gameData, userData, enemyData);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} is currently not in a fight.",
                        Colours.DefaultColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("health")]
        [Alias("hp")]
        [RequiredChannel(346429281314013184)]
        public async Task SelfHealth()
        {
            using (var db = new hanekawaContext())
            {
                var user = Context.User;
                var gamedata = await db.GetOrCreateShipGame(user);
                var health = gamedata.Health - gamedata.Damagetaken;
                var embed = EmbedGenerator.AuthorEmbed($"Health: {health}/{gamedata.Health}", $"{user.Mention}",
                    Colours.DefaultColour, user);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("flee")]
        [Alias("run")]
        [RequiredChannel(346429281314013184)]
        public async Task FleeFromCombat()
        {
            var user = Context.User;
            using (var db = new hanekawaContext())
            {
                var gameData = await db.GetOrCreateShipGame(user);
                gameData.Combatstatus = 0;
                gameData.EnemyDamageTaken = 0;
                await db.SaveChangesAsync();

                var embed = EmbedGenerator.DefaultEmbed($"{user.Username} has fleed from combat",
                    Colours.DefaultColour);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }
    }
}