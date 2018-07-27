using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Entities;
using Jibril.Services.Profile;

namespace Jibril.Modules.Account.Profile
{
    public class Profile : InteractiveBase
    {
        private readonly ProfileBuilder _profileBuilder;

        public Profile(ProfileBuilder profileBuilder)
        {
            _profileBuilder = profileBuilder;
        }

        [Command("profile", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel(339383206669320192)]
        public async Task ProfileAsync(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            await Context.Channel.TriggerTypingAsync();
            var stream = await _profileBuilder.GetProfileAsync(user);
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "profile.png");
            stream.Dispose();
        }

        [Command("preview", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel(339383206669320192)]
        public async Task ProfileAsync(string url)
        {
            try
            {
                var stream = await _profileBuilder.GetProfileAsync((Context.User as SocketGuildUser), url);
                stream.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendFileAsync(stream, "profile.png");
            }
            catch
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply(
                            $"{Context.User.Mention} couldn't make a background with that picture. Please use a direct image and preferably from imgur.com", Color.Red.RawValue)
                        .Build());
            }
        }
    }
}
