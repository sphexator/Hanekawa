using Discord;
using Discord.Commands;
using Discord.Addons.Preconditions;
using Jibril.Preconditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Jibril.Modules.Game.Services;
using Jibril.Services;
using Jibril.Data.Variables;
using Jibril.Services.Common;

namespace Jibril.Modules.Game
{
    public class NPC : ModuleBase<SocketCommandContext>
    {
        [Command("search", RunMode = RunMode.Async)]
        [Alias("find", "radar")]
        [RequiredChannel(346429281314013184)]
        [Ratelimit(12, 1, Measure.Minutes, false, false)]
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
                    var embed = CombatResponse.Combat(user, Colours.OKColour, GameStatus);
                    await ReplyAsync($"{user.Username}, You're already in a fight, use !attack to fight the enemy", false, embed.Build());
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
        [RequiredChannel(346429281314013184)]
        [Ratelimit(12, 1, Measure.Minutes, false, false)]
        public async Task AttackTarget()
        {
            var user = Context.User;
            var gameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
            if(gameData.Combatstatus == 1)
            {
                var userData = DatabaseService.UserData(user).FirstOrDefault();
                var enemyData = GameDatabase.Enemy(gameData.Enemyid).FirstOrDefault();
                var embed = Combat.CombatDamage(user, gameData, userData, enemyData);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} is currently not in a fight.", Colours.DefaultColour);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }
    }
}
