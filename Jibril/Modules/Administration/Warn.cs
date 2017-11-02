﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Administration.Services;
using Jibril.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Administration
{
    public class Warn : ModuleBase<SocketCommandContext>
    {
        [Command("warn")]
        [Alias("warning", "w")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task WarnUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            var userCheck = AdminDb.CheckExistingUserWarn(user);
            var dm = await user.GetOrCreateDMChannelAsync();
            if (userCheck.Count <= 0 && user.IsBot != true)
            {
                WarningDB.EnterUser(user);
                AdminDb.CreateWarn(user);
                if (reason == null)
                {
                    WarningDB.AddWarn(user, Context.User, "No Reason Provided");
                    await Context.Message.DeleteAsync().ConfigureAwait(false);
                    var content = $"{Context.User} warned {user}.";
                    var embed = EmbedGenerator.DefaultEmbed(content, Colours.OKColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                    await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                        $"\n" +
                        $"Staff: {Context.User}\n" +
                        $"Reason: N/A").ConfigureAwait(false);
                }
                else
                {
                    WarningDB.AddWarn(user, Context.User, reason.Replace("'", ""));
                    await Context.Message.DeleteAsync().ConfigureAwait(false);
                    var content = $"{Context.User} warned {user}.";
                    var embed = EmbedGenerator.DefaultEmbed(content, Colours.OKColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                    await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                        $"\n" +
                        $"Staff: {Context.User}\n" +
                        $"Reason: {reason}").ConfigureAwait(false);
                }
            }
            else
            {
                var result = AdminDb.GetWarnings(user).FirstOrDefault();
                if (reason == null)
                {
                    WarningDB.AddWarn(user, Context.User, "No reason provided");
                    AdminDb.AddWarn(user);
                    await Context.Message.DeleteAsync();
                    if ((result.Warnings++) == 3)
                    {
                        var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                        await user.AddRoleAsync(muteRole).ConfigureAwait(false);
                        await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);

                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OKColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                        await dm.SendMessageAsync($"You've been warned on KanColle! & threshold for mute has been met.\n" +
                            $"\n" +
                            $"Staff: {Context.User}\n" +
                            $"Reason: N/A").ConfigureAwait(false);
                    }
                    else
                    {
                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OKColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                        await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                            $"\n" +
                            $"Staff: {Context.User}\n" +
                            $"Reason: N/A").ConfigureAwait(false);
                    }
                }
                else
                {
                    WarningDB.AddWarn(user, Context.User, reason.Replace("'", ""));
                    AdminDb.AddWarn(user);
                    await Context.Message.DeleteAsync();
                    if (result.Warnings++ == 3)
                    {
                        var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                        await user.AddRoleAsync(muteRole).ConfigureAwait(false);
                        await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OKColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                        await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                            $"\n" +
                            $"Staff: {Context.User}\n" +
                            $"Reason: {reason}").ConfigureAwait(false);
                    }
                    else
                    {
                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OKColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                        await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                            $"\n" +
                            $"Staff: {Context.User}\n" +
                            $"Reason: {reason}").ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("warnlog")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task WarnLog(SocketGuildUser user)
        {
            var result = AdminDb.GetWarnings(user).FirstOrDefault();
            if (result != null)
            {
                var list = WarningDB.WarnList(user).ToList();
                EmbedBuilder embed = new EmbedBuilder();
                EmbedAuthorBuilder author = new EmbedAuthorBuilder();

                author.WithIconUrl(user.GetAvatarUrl());
                author.WithName(user.Username);
                embed.WithAuthor(author);
                embed.WithColor(new Color(Colours.DefaultColour));
                embed.AddField(y =>
                {
                    y.Name = "Warnings";
                    y.Value = $"{result.Warnings}";
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "Total warnings";
                    y.Value = $"{result.Total_warnings}";
                    y.IsInline = true;
                });
                for (var i = 0; i < result.Warnings; i++)
                {
                    var c = list[i];
                    var warn = i + 1;
                    embed.AddField(y =>
                    {
                        y.Name = $"Warn: {warn}";
                        y.Value = $"<@!{c.Staff_id}>\n" +
                        $"{c.Message}\n" +
                        $"{c.Date}";
                        y.IsInline = false;
                    });
                }
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }
    }
}