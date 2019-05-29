﻿using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account
{
    public partial class Account
    {
        [Name("Profile")]
        [Command("profile")]
        [Description("Showcase yours or another persons profile")]
        [Remarks("profile")]
        [RequiredChannel]
        public async Task ProfileAsync(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User;
            await Context.Channel.TriggerTypingAsync();
            using (var db = new DbService())
            using (var image = await _image.ProfileBuilder(user, db))
            {
                image.Position = 0;
                await Context.Channel.SendFileAsync(image, "profile.png", null);
            }
        }
    }
}