using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Jibril.Data.Variables;
using Jibril.Modules.Administration.Services;
using Jibril.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Administration
{
    public class Warn : InteractiveBase
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
                    var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                    await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(10)).ConfigureAwait(false);
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
                    var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                    await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(10)).ConfigureAwait(false);
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
                    if (result.Warnings++ == 3)
                    {
                        var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                        await user.AddRoleAsync(muteRole).ConfigureAwait(false);
                        await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);

                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                        await dm.SendMessageAsync(
                            $"You've been warned on KanColle! & threshold for mute has been met.\n" +
                            $"\n" +
                            $"Staff: {Context.User}\n" +
                            $"Reason: N/A").ConfigureAwait(false);
                    }
                    else
                    {
                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(10)).ConfigureAwait(false);
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
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                        await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                                                  $"\n" +
                                                  $"Staff: {Context.User}\n" +
                                                  $"Reason: {reason}").ConfigureAwait(false);
                    }
                    else
                    {
                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(10)).ConfigureAwait(false);
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
            var embed = WarnLogEmbed(user);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        private static EmbedBuilder WarnLogEmbed(IGuildUser user)
        {
            var result = AdminDb.GetWarnings(user).FirstOrDefault();
            var list = WarningDB.WarnList(user);
            var userdata = DatabaseService.UserData(user).FirstOrDefault();

            var content = $"**>User Information**\n" +
                          $"Status: {user.Status}\n" +
                          $"Game: {user.Activity}\n" +
                          $"Created: {user.CreatedAt.Humanize()}({user.CreatedAt})\n" +
                          $"\n" +
                          $"**>Member Information**\n" +
                          $"Joined: {user.JoinedAt.Value.Date.Humanize()}({user.JoinedAt})\n" +
                          $"Roles: \n" +
                          $"\n" +
                          $"**>Activity**\n" +
                          $"Last Message: \n" +
                          $"First Message:\n" +
                          $"\n" +
                          $"**>Voice**\n" +
                          $"Sessions:\n" +
                          $"Time:";
            var author = new EmbedAuthorBuilder
            {
                IconUrl = user.GetAvatarUrl(),
                Name = $"{user.Username}#{user.DiscriminatorValue} ({user.Mention})"
            };
            var embed = new EmbedBuilder
            {
                Color = new Color(Colours.DefaultColour),
                Author = author,
                Description = content,
                ThumbnailUrl = user.GetAvatarUrl()
            };
            embed.AddField(y =>
            {
                y.Name = "Warnings";
                y.Value = result == null ? "0" : $"{result.Warnings}";
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Total warnings";
                y.Value = result == null ? "0" : $"{result.Total_warnings}";
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = $"Toxicity value [msg:{userdata.Toxicitymsgcount}]";
                y.Value = userdata != null ? $"{userdata?.Toxicityavg}" : $"0";
                y.IsInline = true;
            });
            if (result == null) return embed;
            {
                var i = 1;
                foreach (var wable in list)
                    try
                    {
                        var nr = i;
                        embed.AddField(y =>
                        {
                            y.Name = $"Warn: {nr}";
                            y.Value = $"<@!{wable.Staff_id}>\n" +
                                      $"{wable.Message}\n" +
                                      $"{wable.Date}";
                            y.IsInline = false;
                        });
                        i++;
                    }
                    catch
                    {
                        // Ignore
                    }
            }
            return embed;
        }

    }
}