using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Jibril.Services.Common;
using Jibril.Services.Welcome.Services;

namespace Jibril.Services.Welcome
{
    public class WelcomeService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;

        public WelcomeService(IServiceProvider provider, DiscordSocketClient discord)
        {
            _discord = discord;
            _provider = provider;

            _discord.UserJoined += Welcomer;
        }

        private Task Welcomer(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                if (user.IsBot != true)
                {
                    //var checkCd = WelcomeCooldown.WelcCd(user);
                    //if (checkCd == false) return;

                    DatabaseInsert.InserToDb(user);
                    var randomString = RandomStringGenerator.StringGenerator();
                    var avatarToLoad = await ImageGenerator.AvatarGenerator(user, randomString);
                    var image = WelcImgGen.WelcomeImageGeneratorAsync(user, avatarToLoad, randomString);
                    var imgstr = image;
                    var guild = user.Guild;
                    var channel = guild.GetTextChannel(339371997802790913);
                    await channel.SendFileAsync(imgstr, $"Welcome {user.Mention} to KanColle! Check out <#339370914724446208> and get a colour role at <#339380254097539072>");
                    RemoveImage.WelcomeFileDelete();
                }
            });
            return Task.CompletedTask;
        }
    }
}