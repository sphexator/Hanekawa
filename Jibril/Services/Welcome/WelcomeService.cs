using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Gambling.Services;
using Jibril.Modules.Game.Services;
using Jibril.Services.Common;
using Jibril.Services.Welcome.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Services.Welcome
{
    public class WelcomeService
    {
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _provider;

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
                DatabaseService.EnterUser(user);
                GameDatabase.CreateGameDBEntry(user);
                GambleDB.CreateInventory(user);
                var avatarToLoad = await ImageGenerator.AvatarGenerator(user);
                var image = WelcImgGen.WelcomeImageGeneratorAsync(user, avatarToLoad);
                var imgstr = image.ToString();
                var guild = _discord.GetGuild(339370914724446208);
                var channel = guild.GetTextChannel(339371997802790913);
                await channel.SendFileAsync(@"Data\Images\Welcome\Cache\Banner\welcome.png", "");
            });
            return Task.CompletedTask;
        }
    }
}
