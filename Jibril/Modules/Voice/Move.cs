using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Preconditions;
using System.Linq;
using Jibril.Services;
using Jibril.Services.Level.Lists;

namespace Jibril.Modules.Voice
{
    public class Move : InteractiveBase
    {
        [Command("move", RunMode = RunMode.Async)]
        [UserMustBeInVoice]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task MoveUser(SocketGuildUser user)
        {
            var vcUsers = await (Context.User as IVoiceState).VoiceChannel.GetUsersAsync().ToList();
            var users = new List<UserData>();
            foreach (var vcu in vcUsers)
            {
                var dataUser = DatabaseService.UserData((vcu as IUser));
                users.AddRange(dataUser);
            }
        }
    }
}
