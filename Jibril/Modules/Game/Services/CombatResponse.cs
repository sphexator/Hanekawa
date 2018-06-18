using System.Linq;
using System.Threading.Tasks;
using Discord;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Services;
using Jibril.Services.Level.Lists;

namespace Jibril.Modules.Game.Services
{
    public class CombatResponse
    {
        public static async Task<EmbedBuilder> Combat(IUser user, uint Colour, Shipgame gameStatus)
        {
            using (var db = new hanekawaContext())
            {
                var userData = await db.GetOrCreateUserData(user);
                var enemyData = await db.Enemyidentity.FindAsync(gameStatus.Enemyid.Value);
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(Colour));
                embed.WithImageUrl($"https://i.imgur.com/{enemyData.ImageName}.png");

                var fieldClass = new EmbedFieldBuilder();
                var fieldHealth = new EmbedFieldBuilder();
                var fieldLevel = new EmbedFieldBuilder();

                fieldClass.WithIsInline(true);
                fieldClass.WithName("Class");
                fieldClass.WithValue($"{enemyData.EnemyClass}");

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

        }

        public static async Task<EmbedBuilder> CombatStartAsync(IUser user, uint Colour, int enemy, int enemyHealth, Exp userData,
            Enemyidentity enemyName)
        {
            using (var db = new hanekawaContext())
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(Colour));
                //embed.WithTitle(enemyName.FirstOrDefault().enemyName);
                embed.WithImageUrl($"http://i.imgur.com/{enemyName.ImageName}.png");

                var fieldClass = new EmbedFieldBuilder();
                var fieldHealth = new EmbedFieldBuilder();
                var fieldLevel = new EmbedFieldBuilder();

                var GetGameData = await db.GetOrCreateShipGame(user);
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

        public static EmbedBuilder CombatResponseMessage(Enemyidentity enemyData, uint Colour, string Content, string Usr,
            string Enemy)
        {
            var embed = new EmbedBuilder();
            var authorBuilder = new EmbedAuthorBuilder();
            var userHealth = new EmbedFieldBuilder();
            var enemyHealth = new EmbedFieldBuilder();

            authorBuilder.WithName($"{enemyData.EnemyName}");
            authorBuilder.WithIconUrl($"http://i.imgur.com/{enemyData.ImageName}.png");

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