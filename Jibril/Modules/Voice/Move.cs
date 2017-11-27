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
            if ((Context.User as IVoiceState) == null) return;
            var vcUsers = await (Context.User as IVoiceState).VoiceChannel.GetUsersAsync().ToList();
            var users = new List<UserData>();
            foreach (var vcu in vcUsers)
            {
                var dataUser = DatabaseService.UserData((vcu as IUser));
                users.AddRange(dataUser);
            }
            var mu = users.OrderByDescending(x => x.Voice_timer).FirstOrDefault();
            var mui = Convert.ToUInt64(mu.UserId);
            if (mui == Context.User.Id)
            {
                await user.ModifyAsync(x => x.ChannelId = (Context.User as IVoiceState).VoiceChannel.Id);
            } 
        }
    }
}
