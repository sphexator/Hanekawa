using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;

namespace Hanekawa.Modules.Audio
{
    public class Move : InteractiveBase
    {
        [Command("move", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task MoveUser(SocketGuildUser mvUser)
        {
            if (Context.User.Id == mvUser.Id) return;
            if ((Context.User as IVoiceState)?.VoiceChannel == null)
            {
                await Context.ReplyAsync(
                    $"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                    Color.Red.RawValue);
                return;
            }

            if ((mvUser as IVoiceState).VoiceChannel == null)
            {
                await Context.ReplyAsync($"{mvUser.Mention}, you need to be in a voice channel to use this command.",
                    Color.Red.RawValue);
                return;
            }

            using (var db = new DbService())
            {
                var users = new List<Addons.Database.Tables.Account.Account>();
                foreach (var x in await ((IVoiceState) Context.User).VoiceChannel.GetUsersAsync().ToArray())
                {
                    var user = x.FirstOrDefault();
                    if (user == null) return;
                    if (user.IsBot) return;
                    users.Add(await db.GetOrCreateUserData(user as SocketGuildUser));
                }

                var qual = users.OrderByDescending(x => x.ChannelVoiceTime).First();
                if (qual.UserId != Context.User.Id)
                {
                    await Context.ReplyAsync(
                        $"{Context.User.Mention} can't use this command. Talk to {Context.Guild.GetUser(qual.UserId).Mention}",
                        Color.Red.RawValue);
                    return;
                }

                await ReplyAsync(
                    $"{Context.User.Mention} wants to move {mvUser.Mention} to {((IVoiceState) Context.User).VoiceChannel.Name}, do you accept? (y/n)");
                var status = true;
                while (status)
                    try
                    {
                        var response = await NextMessageAsync(new EnsureFromUserCriterion(mvUser.Id),
                            TimeSpan.FromSeconds(60));
                        if (response.Content.ToLower() != "y") status = false;
                        if (response.Content.ToLower() != "n") continue;
                        await Context.ReplyAsync($"{mvUser.Mention} didn't accept or respond in time.",
                            Color.Red.RawValue);
                        return;
                    }
                    catch
                    {
                        await Context.ReplyAsync("Move request timed out", Color.Red.RawValue);
                        return;
                    }

                await mvUser.ModifyAsync(x => x.ChannelId = ((IVoiceState) Context.User).VoiceChannel.Id);
                await Context.ReplyAsync($"Moved {mvUser.Mention} to {((IVoiceState) Context.User).VoiceChannel.Name}",
                    Color.Green.RawValue);
            }
        }
    }
}