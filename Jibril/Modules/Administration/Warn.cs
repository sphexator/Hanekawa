using System;
using System.Collections.Generic;
using System.IO;
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
        [Command("warn", RunMode = RunMode.Async)]
        [Alias("warning", "w")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task WarnUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var userCheck = AdminDb.CheckExistingUserWarn(user);
            var dm = await user.GetOrCreateDMChannelAsync();
            var datetimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            var msgs = (await Context.Channel.GetMessagesAsync().FlattenAsync()).Where(m => m.Author.Id == user.Id)
                .Take(100).ToArray();
            var msgLog = msgs.ToList();

            if (userCheck.Count <= 0 && user.IsBot != true)
            {
                WarningDB.EnterUser(user);
                AdminDb.CreateWarn(user);
                if (reason == null)
                {
                    WarningDB.AddWarn(user, Context.User, "No Reason Provided", datetimeString);
                    var content = $"{Context.User} warned {user}.";
                    var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                    await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                    await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                                              $"\n" +
                                              $"Staff: {Context.User}\n" +
                                              $"Reason: N/A").ConfigureAwait(false);
                    await StoreMessages(datetimeString, user, msgLog);
                }
                else
                {
                    WarningDB.AddWarn(user, Context.User, reason.Replace("'", ""), datetimeString);
                    var content = $"{Context.User} warned {user}.";
                    var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                    await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                    await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                                              $"\n" +
                                              $"Staff: {Context.User}\n" +
                                              $"Reason: {reason}").ConfigureAwait(false);
                    await StoreMessages(datetimeString, user, msgLog);
                }
            }
            else
            {
                var result = AdminDb.GetWarnings(user).FirstOrDefault();
                if (reason == null)
                {
                    WarningDB.AddWarn(user, Context.User, "No reason provided", datetimeString);
                    AdminDb.AddWarn(user);
                    if (result.Warnings++ == 3)
                    {
                        var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                        await user.AddRoleAsync(muteRole).ConfigureAwait(false);
                        await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);

                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        await dm.SendMessageAsync(
                            $"You've been warned on KanColle! & threshold for mute has been met.\n" +
                            $"\n" +
                            $"Staff: {Context.User}\n" +
                            $"Reason: N/A").ConfigureAwait(false);
                        await StoreMessages(datetimeString, user, msgLog);
                    }
                    else
                    {
                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                                                  $"\n" +
                                                  $"Staff: {Context.User}\n" +
                                                  $"Reason: N/A").ConfigureAwait(false);
                        await StoreMessages(datetimeString, user, msgLog);
                    }
                }
                else
                {
                    WarningDB.AddWarn(user, Context.User, reason.Replace("'", ""), datetimeString);
                    AdminDb.AddWarn(user);
                    if (result.Warnings++ == 3)
                    {
                        var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                        await user.AddRoleAsync(muteRole).ConfigureAwait(false);
                        await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                                                  $"\n" +
                                                  $"Staff: {Context.User}\n" +
                                                  $"Reason: {reason}").ConfigureAwait(false);
                        await StoreMessages(datetimeString, user, msgLog);
                    }
                    else
                    {
                        var content = $"{Context.User} warned {user}.";
                        var embed = EmbedGenerator.DefaultEmbed(content, Colours.OkColour);
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        await dm.SendMessageAsync($"You've been warned on KanColle!\n" +
                                                  $"\n" +
                                                  $"Staff: {Context.User}\n" +
                                                  $"Reason: {reason}").ConfigureAwait(false);
                        await StoreMessages(datetimeString, user, msgLog);
                    }
                }
            }

        }

        [Command("warnlog")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task WarnLog(SocketGuildUser user)
        {
            var result = AdminDb.GetWarnings(user).FirstOrDefault();
            var warnNr = result?.Warnings ?? 0;
            var embed = WarnLogEmbed(user, Context, warnNr);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("fullwarnlog")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AdminWarnlog(SocketGuildUser user)
        {
            var result = AdminDb.GetWarnings(user).FirstOrDefault();
            var embed = WarnLogEmbed(user, Context, result.TotalWarnings);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("msglog", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task MsgLog(SocketGuildUser user, int id)
        {
            var stream = GetMsgLog(user, id);
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "log.txt", $"Logs for {user.Nickname ?? user.Username}({user.Id}) with warnId {id}.");
        }

        private static EmbedBuilder WarnLogEmbed(IGuildUser user, SocketCommandContext context, uint limit)
        {
            var result = AdminDb.GetWarnings(user).FirstOrDefault();
            var userdata = DatabaseService.UserData(user).FirstOrDefault();

            var roleList = new List<string>();
            foreach (var x in user.RoleIds)
            {
                var role = context.Guild.Roles.First(y => y.Id == x);
                if (role.Name == "@everyone") continue;
                roleList.Add(role.Name);
            }

            var roles = string.Join(", ", roleList);
            var content = $"**>User Information**\n" +
                          $"Status: {user.Status}\n" +
                          $"Game: {user.Activity}\n" +
                          $"Created: {user.CreatedAt.Humanize()}({user.CreatedAt})\n" +
                          $"\n" +
                          $"**>Member Information**\n" +
                          $"Joined: {user.JoinedAt.Value.Date.Humanize()}({user.JoinedAt})\n" +
                          $"Roles: {roles}\n" +
                          $"\n" +
                          $"**>Activity**\n" +
                          $"Last Message: {userdata.LastMsg.Humanize()} \n" +
                          $"First Message: {userdata.FirstMsg.Humanize()}";

            var author = new EmbedAuthorBuilder
            {
                IconUrl = user.GetAvatarUrl(),
                Name = $"{user.Username}#{user.DiscriminatorValue} ({user.Id})"
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
                y.Value = result == null ? "0" : $"{result.TotalWarnings}";
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = $"Toxicity value [msg:{userdata.Toxicitymsgcount}]";
                y.Value = userdata != null ? $"{userdata?.Toxicityavg}" : $"0";
                y.IsInline = true;
            });
            if (limit == 0) return embed;
            var list = WarningDB.WarnList(user, limit);
            if (result == null ) return embed;
            {
                foreach (var wable in list)
                    try
                    {
                        embed.AddField(y =>
                        {
                            y.Name = $"Warn: {wable.Id}";
                            y.Value = $"<@!{wable.StaffId}>\n" +
                                      $"{wable.Message}\n" +
                                      $"{wable.Date}";
                            y.IsInline = false;
                        });
                    }
                    catch
                    {
                        // Ignore
                    }
            }
            return embed;
        }

        private static Task StoreMessages(string datetimeString, IUser user, IEnumerable<IMessage> msgLog)
        {
            try
            {
                var warnId = WarningDB.GetWarnId(user, datetimeString).FirstOrDefault();
                foreach (var x in msgLog)
                {
                    try
                    {
                        AdminDb.AddMsgLog(x, warnId.Id);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            catch
            {
                //ignore
            }
            return Task.CompletedTask;
        }

        private static MemoryStream GetMsgLog(IUser user, int id)
        {
            var messages = AdminDb.GetMsgLogs(user, id);
            var stream = new MemoryStream();
            var tw = new StreamWriter(stream);

            tw.WriteLine($"Logs for {user.Username}({user.Id})");
            foreach (var x in messages)
            {
                tw.WriteLine($"{x.Date} - {x.Author}: {x.Message}");
            }
            return stream;
        }
    }
}