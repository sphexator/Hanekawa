using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Jibril.Services.Common;
using Jibril.Services.Welcome.Services;

namespace Jibril.Services.Welcome
{
    public class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly bool _disableBanner;

        public WelcomeService(IServiceProvider provider, DiscordSocketClient discord)
        {
            _client = discord;
            _provider = provider;

            _client.UserJoined += Welcomer;

            _disableBanner = false;
        }

        private Task Welcomer(SocketGuildUser user)
        {
            if (_disableBanner) return Task.CompletedTask;
            var _ = Task.Run(async () =>
            {
                if (user.IsBot != true)
                {
                    var randomString = RandomStringGenerator.StringGenerator();
                    var avatarToLoad = await ImageGenerator.AvatarGenerator(user, randomString);
                    var image = WelcImgGen.WelcomeImageGeneratorAsync(user, avatarToLoad, randomString);
                    var channel = user.Guild.GetTextChannel(339371997802790913);
                    await channel.SendFileAsync(image, $"Welcome {user.Mention} to KanColle! Check out <#339370914724446208> and get a colour role at <#339380254097539072>");
                    RemoveImage.WelcomeFileDelete();
                }
            });
            return Task.CompletedTask;
        }
    }
}