using System.Linq;
using Discord;
using Jibril.Data.Variables;
using Jibril.Services;
using Jibril.Services.Level.Lists;

namespace Jibril.Modules.Game.Services
{
    public class CombatResponse
    {
        public static EmbedBuilder Combat(IUser user, uint Colour, GameStatus gameStatus)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var EnemyData = GameDatabase.Enemy(gameStatus.Enemyid).FirstOrDefault();
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(Colour));
            embed.WithImageUrl($"https://i.imgur.com/{EnemyData.ImagePath}.png");

            var fieldClass = new EmbedFieldBuilder();
            var fieldHealth = new EmbedFieldBuilder();
            var fieldLevel = new EmbedFieldBuilder();

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

        public static EmbedBuilder CombatStart(IUser user, uint Colour, int enemy, int enemyHealth, UserData userData,
            EnemyId enemyName)
        {
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(Colour));
            //embed.WithTitle(enemyName.FirstOrDefault().enemyName);
            embed.WithImageUrl($"http://i.imgur.com/{enemyName.ImagePath}.png");

            var fieldClass = new EmbedFieldBuilder();
            var fieldHealth = new EmbedFieldBuilder();
            var fieldLevel = new EmbedFieldBuilder();

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

        public static EmbedBuilder CombatResponseMessage(EnemyId enemyData, uint Colour, string Content, string Usr,
            string Enemy)
        {
            var embed = new EmbedBuilder();
            var authorBuilder = new EmbedAuthorBuilder();
            var userHealth = new EmbedFieldBuilder();
            var enemyHealth = new EmbedFieldBuilder();

            authorBuilder.WithName($"{enemyData.EnemyName}");
            authorBuilder.WithIconUrl($"http://i.imgur.com/{enemyData.ImagePath}.png");

            userHealth.WithName("Your Health");
            userHealth.WithValue(Usr);
            userHealth.WithIsInline(true);

            enemyHealth.WithName("Enemy Health");
            enemyHealth.WithValue(Enemy);
            enemyHealth.WithIsInline(true);

            embed.WithColor(new Color(Colour));
            embed.WithAuthor(authorBuilder);
            embed.AddField(userHealth);
            embed.AddField(enemyHealth);
            embed.WithDescription(Content);

            return embed;
        }
    }
}