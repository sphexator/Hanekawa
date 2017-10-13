using Discord;
using Jibril.Data.Variables;
using Jibril.Services;
using Jibril.Services.Common;
using Jibril.Services.Level.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jibril.Modules.Game.Services
{
    public class FindEnemy
    {
        public static EmbedBuilder FindEnemyNPC(IUser user, UserData userData)
        {
            Random rand = new Random();
            int chance = rand.Next(1, 101);
            if (chance >= 40)
            {
                var enemy = rand.Next(1, 20);
                var enemyData = GameDatabase.Enemy(enemy).FirstOrDefault();
                var enemyHealth = EnemyStat.HealthPoint(userData.Level, enemyData.Health);
                var userHealth = BaseStats.HealthPoint(userData.Level, userData.ShipClass);
                var result = GameDatabase.GameCheckExistingUser(user);

                if (result.Count <= 0) GameDatabase.AddNPCStart(user, enemy, userHealth, enemyHealth);
                else GameDatabase.UpdateNPCStart(user, enemy, userHealth, enemyHealth);

                var embed = CombatResponse.CombatStart(user, Colours.DefaultColour, enemy, enemyHealth, userData, enemyData);
                return embed;
            }
            else if(chance >= 95 && userData.Level >= 40)
            {
                var enemy = rand.Next(21, 26);
                var enemyData = GameDatabase.Enemy(enemy).FirstOrDefault();
                var enemyHealth = EnemyStat.HealthPoint(userData.Level, enemyData.Health);
                var userHealth = BaseStats.HealthPoint(userData.Level, userData.ShipClass);
                var result = GameDatabase.GameCheckExistingUser(user);

                if (result.Count <= 0) GameDatabase.AddNPCStart(user, enemy, userHealth, enemyHealth);
                else GameDatabase.UpdateNPCStart(user, enemy, userHealth, enemyHealth);

                var embed = CombatResponse.CombatStart(user, Colours.DefaultColour, enemy, enemyHealth, userData, enemyData);
                return embed;
            }
            else
            {
                var Embed = EmbedGenerator.DefaultEmbed($"{user.Username} searched throughout the sea and found no enemy", Colours.DefaultColour);
                return Embed;
            }
        }
    }
}
