﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;
using Jibril.Services.Level.Lists;

namespace Jibril.Modules.Voice
{
    public class Move : InteractiveBase
    {
        [Command("move", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [UserMustBeInVoice]
        public async Task MoveUser(SocketGuildUser user)
        {
            try
            {
                var vcUsers = await (Context.User as IVoiceState).VoiceChannel.GetUsersAsync().ToArray();
                var users = new List<UserData>();
                for (var i = 0; i <= vcUsers.Length; i++)
                    try
                    {
                        if (vcUsers[i].First().IsBot != true && vcUsers[i].First() != null)
                        {
                            var dbuser = vcUsers[i].First();
                            var dataUser = DatabaseService.UserData(dbuser);
                            users.AddRange(dataUser);
                        }
                    }
                    catch (Exception a)
                    {
                        Console.Write(a.Message);
                    }

                var mu = users.OrderByDescending(x => x.Voice_timer).First();
                var mui = Convert.ToUInt64(mu.UserId);
                if (mui == Context.User.Id)
                {
                    var confirmEmbed = EmbedGenerator.DefaultEmbed(
                        $"{Context.User.Mention} wants to move {user.Mention}, do you accept? (Y/N)",
                        Colours.DefaultColour);
                    await ReplyAsync("", false, confirmEmbed.Build());
                    var response =
                        await NextMessageAsync(new EnsureFromUserCriterion(user.Id), TimeSpan.FromSeconds(20));
                    if (response.Content.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (Context.User as IVoiceState == null) return;
                        await user.ModifyAsync(x => x.ChannelId = (Context.User as IVoiceState).VoiceChannel.Id);
                        var embed = EmbedGenerator.DefaultEmbed(
                            $"Moved {user.Username} to {Context.User.Username} voice channel", Colours.OkColour);
                        await ReplyAsync("", false, embed.Build());
                    }
                    else
                    {
                        var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} didn't respond in time.",
                            Colours.FailColour);
                        await ReplyAsync("", false, embed.Build());
                    }
                }
                else
                {
                    var embed = EmbedGenerator.DefaultEmbed($"You cannot use this command. Ask <@{mui}> instead.",
                        Colours.FailColour);
                    await ReplyAsync("", false, embed.Build());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}