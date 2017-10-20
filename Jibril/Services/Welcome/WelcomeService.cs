using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
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
                var image = WelcImgGen.WelcomeImageGeneratorAsync(user).ToString();
                var guild = _discord.GetGuild(234505708861652993);
                var channel = guild.GetTextChannel(360140270605434882);
                await channel.SendFileAsync(image, "");
            });
            return Task.CompletedTask;
        }
    }
}
