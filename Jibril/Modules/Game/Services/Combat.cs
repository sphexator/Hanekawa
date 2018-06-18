using Discord;
using Jibril.Data.Variables;
using System;
using System.Threading.Tasks;

namespace Jibril.Modules.Game.Services
{
    public class Combat
    {
        public static async Task<EmbedBuilder> CombatDamageAsync(IUser user, Shipgame gameData, Exp userData, Enemyidentity enemyData)
        {
            using (var db = new hanekawaContext())
            {
                var userDamage = BaseStats.CriticalStrike(userData.ShipClass, userData.Level);
                var enemyDamage = EnemyStat.Avoidance(userData.ShipClass, userData.Level);
                if (gameData.Enemyhealth >= gameData.Enemyhealth - (gameData.EnemyDamageTaken + userDamage))
                {
                    if (gameData.Health <= gameData.Damagetaken + enemyDamage)
                    {
                        // You died
                        var enemyTotalHp = $"{gameData.Enemyhealth - (gameData.EnemyDamageTaken + userDamage)}/{gameData.Enemyhealth}";
                        var userTotalHp = $"0/{gameData.Health}";
                        var content = $"**{user.Username}** hit **{enemyData.EnemyName}** for **{userDamage}** damage\n" +
                                      $"\n" +
                                      $"**{enemyData.EnemyName}** counter attacked for **{enemyDamage}** damage and killed {user.Username}" +
                                      $"\n" +
                                      $"{user.Username} died, please repair using !repair";
                        var embed = CombatResponse.CombatResponseMessage(enemyData, Colours.FailColour, content,
                            userTotalHp, enemyTotalHp);

                        gameData.Combatstatus = 0;
                        gameData.EnemyDamageTaken = 0;
                        await db.SaveChangesAsync();

                        return embed;
                    }
                    else
                    {
                        // Apply damage
                        var enemyTotalHp = $"{gameData.Enemyhealth - (gameData.EnemyDamageTaken + userDamage)}/{gameData.Enemyhealth}";
                        var userTotalHp = $"{gameData.Health <= gameData.Damagetaken + enemyDamage}/{gameData.Health}";
                        var content = $"**{user.Username}** hit **{enemyData.EnemyName}** for **{userDamage}** damage\n" +
                                      $"\n" +
                                      $"**{enemyData.EnemyName}** counter attacked for **{enemyDamage}** damage";
                        var embed = CombatResponse.CombatResponseMessage(enemyData, Colours.DefaultColour, content,
                            userTotalHp, enemyTotalHp);

                        gameData.EnemyDamageTaken = gameData.EnemyDamageTaken + userDamage;
                        gameData.Damagetaken = gameData.Damagetaken + enemyDamage;
                        await db.SaveChangesAsync();

                        return embed;
                    }
                }
                else
                {
                    // Killed enemy
                    var enemyTotalHp = $"0/{gameData.Enemyhealth}";
                    var userTotalHp = $"{gameData.Health <= gameData.Damagetaken + enemyDamage}/{gameData.Health}";
                    var content = $"**{user.Username}** hit and killed **{enemyData.EnemyName}** for **{userDamage}**\n" +
                                  $"\n" +
                                  $"**{user.Username}** received **{enemyData.ExpGain}** experience and **${enemyData.CurrencyGain}** for killing **{enemyData.EnemyName}**";
                    var embed = CombatResponse.CombatResponseMessage(enemyData, Colours.OkColour, content, userTotalHp,
                        enemyTotalHp);

                    userData.Xp = userData.Xp + enemyData.ExpGain.Value;
                    userData.Tokens = userData.Tokens + Convert.ToUInt32(enemyData.CurrencyGain.Value);

                    gameData.Combatstatus = 0;
                    gameData.KillAmount = gameData.KillAmount + 1;
                    gameData.EnemyDamageTaken = 0;
                    return embed;
                }
            }
        }
    }
}