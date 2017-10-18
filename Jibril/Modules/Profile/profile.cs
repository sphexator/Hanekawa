using Discord.Commands;
using Jibril.Modules.Profile.Services;
using Jibril.Preconditions;
using Jibril.Services.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Profile
{
    public class Profile : ModuleBase<SocketCommandContext>
    {
        [Command("Profile", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        public async Task PostProfile()
        {
            var user = Context.User;
            var profile = ProfileCreator.PfpCreator(user);
            await Context.Channel.SendFileAsync(profile);
        }
    }
}
