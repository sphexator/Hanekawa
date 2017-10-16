using Discord;
using Jibril.Data.Variables;
using Jibril.Services.Level.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jibril.Modules.Game.Services
{
    public class Combat
    {
        public static EmbedBuilder CombatDamage(IUser user, GameStatus gameData, UserData userData, EnemyId enemyData)
        {
            var userDamage = BaseStats.CriticalStrike(userData.ShipClass, userData.Level);
            var enemyDamage = EnemyStat.Avoidance(userData.ShipClass, userData.Level);
            GameDatabase.EnemyDamageTaken(userDamage, user);
            var enemyHealth = gameData.Enemyhealth - gameData.EnemyDamageTaken;
            var afterUserDmg = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
            if(gameData.Enemyhealth > afterUserDmg.Damagetaken)
            {
                GameDatabase.UserDamageTaken(enemyDamage, user);
                var afterDmg = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
                if(afterDmg.Health < afterDmg.Damagetaken)
                {
                    var enemyHealthafter = afterDmg.Enemyhealth - afterDmg.EnemyDamageTaken;
                    var enemyTotalHp = $"{enemyHealthafter}/{gameData.Enemyhealth}";
                    var userTotalHp = $"0/{gameData.Health}";
                    var content = $"**{user.Username}** hit **{enemyData.EnemyName}** for **{userDamage}** damage\n" +
                        $"\n" +
                        $"**{enemyData.EnemyName}** counter attacked for **{enemyDamage}** damage and killed {user.Username}" +
                        $"\n" +
                        $"{user.Username} died, please repair using !repair";
                    var embed = CombatResponse.CombatResponseMessage(enemyData, Colours.FailColour, content, userTotalHp, enemyTotalHp);
                    // You died
                    GameDatabase.GameOverNPC(user);
                    return embed;
                }
                else
                {
                    var enemyHealthafter = afterDmg.Enemyhealth - afterDmg.EnemyDamageTaken;
                    var userHealthafter = afterDmg.Health - afterDmg.Damagetaken;
                    var enemyTotalHp = $"{enemyHealthafter}/{gameData.Enemyhealth}";
                    var userTotalHp = $"{userHealthafter}/{gameData.Health}";
                    var content = $"**{user.Username}** hit **{enemyData.EnemyName}** for **{userDamage}** damage\n" +
                        $"\n" +
                        $"**{enemyData.EnemyName}** counter attacked for **{enemyDamage}** damage";
                    var embed = CombatResponse.CombatResponseMessage(enemyData, Colours.DefaultColour, content, userTotalHp, enemyTotalHp);
                    // Apply damage
                    return embed;
                }
            }
            else
            {
                var afterDmg = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
                var userHealthafter = afterDmg.Health - afterDmg.Damagetaken;
                var enemyTotalHp = $"0/{gameData.Enemyhealth}";
                var userTotalHp = $"{userHealthafter}/{gameData.Health}";
                var content = $"**{user.Username}** hit and killed **{enemyData.EnemyName}** for **{userDamage}**\n" +
                    $"\n" +
                    $"**{user.Username}** received **{enemyData.ExpGain}** experience and **${enemyData.CurrenyGain}** for killing **{enemyData.EnemyName}**";
                var embed = CombatResponse.CombatResponseMessage(enemyData, Colours.OKColour, content, userTotalHp, enemyTotalHp);
                // Killed enemy
                GameDatabase.FightOver(enemyData.ExpGain, enemyData.CurrenyGain, user);
                GameDatabase.FinishedNPCFight(user);
                return embed;
            }
        }
    }
}