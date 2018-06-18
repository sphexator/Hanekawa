using Discord;
using Jibril.Data.Variables;
using Jibril.Services.Common;
using Jibril.Services.Level.Lists;
using System;
using System.Linq;
using System.Threading.Tasks;
using Jibril.Extensions;

namespace Jibril.Modules.Game.Services
{
    public class FindEnemy
    {
        public static async Task<EmbedBuilder> FindEnemyNPCAsync(IUser user, Exp userData)
        {
            using (var db = new hanekawaContext())
            {
                var rand = new Random();
                var chance = rand.Next(1, 101);
                if (chance >= 40)
                {
                    var enemy = rand.Next(1, 20);
                    var enemyData = await db.Enemyidentity.FindAsync(enemy);
                    var enemyHealth = EnemyStat.HealthPoint(userData.Level, enemyData.Health.Value);
                    var userHealth = BaseStats.HealthPoint(userData.Level, userData.ShipClass);
                    var shipdata = await db.GetOrCreateShipGame(user);
                    shipdata.Health = userHealth;
                    shipdata.Enemyid = enemyData.Id;
                    shipdata.Health = enemyHealth;
                    shipdata.EnemyDamageTaken = 0;
                    shipdata.Combatstatus = 1;
                    await db.SaveChangesAsync();

                    var embed = await CombatResponse.CombatStartAsync(user, Colours.DefaultColour, enemy, enemyHealth, userData,
                        enemyData);
                    return embed;
                }

                if (chance >= 95 && userData.Level >= 40)
                {
                    var enemy = rand.Next(21, 26);
                    var enemyData = await db.Enemyidentity.FindAsync(enemy);
                    var enemyHealth = EnemyStat.HealthPoint(userData.Level, enemyData.Health.Value);
                    var userHealth = BaseStats.HealthPoint(userData.Level, userData.ShipClass);
                    var shipdata = await db.GetOrCreateShipGame(user);
                    shipdata.Health = userHealth;
                    shipdata.Enemyid = enemyData.Id;
                    shipdata.Health = enemyHealth;
                    shipdata.EnemyDamageTaken = 0;
                    shipdata.Combatstatus = 1;
                    await db.SaveChangesAsync();

                    var embed = await CombatResponse.CombatStartAsync(user, Colours.DefaultColour, enemy, enemyHealth, userData,
                        enemyData);
                    return embed;
                }

                var Embed = EmbedGenerator.DefaultEmbed($"{user.Username} searched throughout the sea and found no enemy",
                    Colours.DefaultColour);
                return Embed;
            }
        }
    }
}