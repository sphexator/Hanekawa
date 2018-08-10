using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Hanekawa.Services.Entities;

namespace Hanekawa.Modules.Audio
{
    public class Move : InteractiveBase
    {
        [Command("move", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [UserMustBeInVoice]
        public async Task MoveUser(SocketGuildUser mvUser)
        {
            using (var db = new DbService())
            {
                var users = new List<Services.Entities.Tables.Account>();
                foreach (var x in await ((IVoiceState)Context.User).VoiceChannel.GetUsersAsync().ToArray())
                {
                    var user = x.FirstOrDefault();
                    if (user == null) return;
                    if (user.IsBot) return;
                    users.Add(await db.GetOrCreateUserData(user as SocketGuildUser));
                }

                var qual = users.OrderByDescending(x => x.ChannelVoiceTime).First();
                if (qual.UserId != Context.User.Id)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply(
                                $"{Context.User.Mention} can't use this command. Talk to {Context.Guild.GetUser(qual.UserId).Mention}",
                                Color.Red.RawValue).Build());
                    return;
                }

                await ReplyAsync(
                    $"{Context.User.Mention} wants to move {mvUser.Mention} to {((IVoiceState) Context.User).VoiceChannel.Name}, do you accept? (y/n)");
                var response = await NextMessageAsync(new EnsureFromUserCriterion(mvUser.Id), TimeSpan.FromSeconds(60));
                if (response.Content.ToLower() != "y")
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply(
                                $"{mvUser.Mention} didn't accept or respond in time.",
                                Color.Red.RawValue).Build());
                    return;
                }

                await mvUser.ModifyAsync(x => x.ChannelId = ((IVoiceState) Context.User).VoiceChannel.Id);
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"Moved {mvUser.Mention} to {((IVoiceState) Context.User).VoiceChannel.Name}",
                            Color.Green.RawValue).Build());
            }
        }
    }
}
