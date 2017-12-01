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
using Google.Apis.Util;
using Jibril.Services;
using Jibril.Services.Level.Lists;

namespace Jibril.Modules.Voice
{
    public class Move : InteractiveBase
    {
        [Command("move", RunMode = RunMode.Async)]
        [UserMustBeInVoice]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
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
                await ReplyAsync($"{Context.User.Mention} wants to move {user.Mention}, do you accept? (Y/N)");
                var response = await NextMessageAsync(new EnsureFromUserCriterion(user.Id));
                if (response.Content.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    await user.ModifyAsync(x => x.ChannelId = (Context.User as IVoiceState).VoiceChannel.Id);
                    await ReplyAsync($"Moved {user.Username} to {Context.User.Username} voice channel");
                }
            }
            else await ReplyAsync($"You cannot use this command. Ask <@{mui}> instead.");
        }
    }
}
