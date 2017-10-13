using Discord;
using Jibril.Data.Variables;
using Jibril.Services;
using Jibril.Services.Level.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jibril.Modules.Game.Services
{
    public class CombatResponse
    {
        public static EmbedBuilder Combat(IUser user, uint Colour, GameStatus gameStatus)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var EnemyData = GameDatabase.Enemy(gameStatus.Enemyid).FirstOrDefault();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(new Color(Colour));
            embed.WithImageUrl($"https://i.imgur.com/{EnemyData.ImagePath}.png");

            EmbedFieldBuilder fieldClass = new EmbedFieldBuilder();
            EmbedFieldBuilder fieldHealth = new EmbedFieldBuilder();
            EmbedFieldBuilder fieldLevel = new EmbedFieldBuilder();

            fieldClass.WithIsInline(true);
            fieldClass.WithName("Class");
            fieldClass.WithValue($"{EnemyData.EnemyClass}");

            fieldHealth.WithIsInline(true);
            fieldHealth.WithName("Health");
            var dmgTaken = gameStatus.EnemyDamageTaken;
            var enmyhealth = gameStatus.Enemyhealth - dmgTaken;
            fieldHealth.WithValue($"{enmyhealth}/{gameStatus.Enemyhealth}");

            fieldLevel.WithIsInline(true);
            fieldLevel.WithName("Level");
            fieldLevel.WithValue($"{userData.Level}");

            embed.AddField(fieldClass);
            embed.AddField(fieldHealth);
            embed.AddField(fieldLevel);

            return embed;
        }

        public static EmbedBuilder CombatStart(IUser user,uint Colour, int enemy, int enemyHealth, UserData userData, EnemyId enemyName)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(new Color(Colour));
            //embed.WithTitle(enemyName.FirstOrDefault().enemyName);
            embed.WithImageUrl($"http://i.imgur.com/{enemyName.ImagePath}.png");

            EmbedFieldBuilder fieldClass = new EmbedFieldBuilder();
            EmbedFieldBuilder fieldHealth = new EmbedFieldBuilder();
            EmbedFieldBuilder fieldLevel = new EmbedFieldBuilder();

            var GetGameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
            fieldClass.WithIsInline(true);
            fieldClass.WithName("Class");
            fieldClass.WithValue($"{enemyName.EnemyClass}");

            fieldHealth.WithIsInline(true);
            fieldHealth.WithName("Health");
            var dmgTaken = GetGameData.EnemyDamageTaken;
            var enmyhealth = enemyHealth - dmgTaken;
            fieldHealth.WithValue($"{enmyhealth}/{enemyHealth}");

            fieldLevel.WithIsInline(true);
            fieldLevel.WithName("Level");
            fieldLevel.WithValue($"{userData.Level}");

            embed.AddField(fieldClass);
            embed.AddField(fieldHealth);
            embed.AddField(fieldLevel);
            embed.Title = $"Enemy: {enemyName.EnemyName}";
            embed.Description = $"You encountered enemy: **{enemyName.EnemyName}**.";

            return embed;
        }
    }
}