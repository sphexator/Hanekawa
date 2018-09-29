using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;

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
            if ((Context.User as IVoiceState).VoiceChannel == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            if ((mvUser as IVoiceState).VoiceChannel == null)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"{mvUser.Mention}, you need to be in a voice channel to use this command.",
                            Color.Red.RawValue).Build());
                return;
            }

            using (var db = new DbService())
            {
                var users = new List<Addons.Database.Tables.Account.Account>();
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
                    $"{Context.User.Mention} wants to move {mvUser.Mention} to {((IVoiceState)Context.User).VoiceChannel.Name}, do you accept? (y/n)");
                var status = true;
                while (status)
                {
                    try
                    {
                        var response = await NextMessageAsync(new EnsureFromUserCriterion(mvUser.Id),
                            TimeSpan.FromSeconds(60));
                        if (response.Content.ToLower() != "y")
                        {
                            status = false;
                        }
                        if (response.Content.ToLower() == "n")
                        {
                            await ReplyAsync(null, false,
                                new EmbedBuilder()
                                    .Reply(
                                        $"{mvUser.Mention} didn't accept or respond in time.",
                                        Color.Red.RawValue).Build());
                            return;
                        }
                    }
                    catch
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("Move request timed out", Color.Red.RawValue).Build());
                        return;
                    }
                }
                await mvUser.ModifyAsync(x => x.ChannelId = ((IVoiceState)Context.User).VoiceChannel.Id);
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"Moved {mvUser.Mention} to {((IVoiceState)Context.User).VoiceChannel.Name}",
                            Color.Green.RawValue).Build());
            }
        }
    }
}