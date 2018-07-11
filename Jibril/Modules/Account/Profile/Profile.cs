using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Entities;

namespace Jibril.Modules.Account.Profile
{
    public class Profile : InteractiveBase
    {
        [Command("profile", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task ProfileAsync(SocketGuildUser user = null, string url = null)
        {
            if(user == null) user = Context.User as SocketGuildUser;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                if (url == null) url = userdata.ProfilePic;

            }
        }
    }
}
