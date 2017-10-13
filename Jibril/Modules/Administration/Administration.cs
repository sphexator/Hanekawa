using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Administration
{
    public class Administration : InteractiveBase
    {
        [Command("prune")]
        [Alias("Prune")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ClearMessage([Remainder] int x = 0)
        {
            if (x <= 2000)
            {
                var messagesToDelete = await Context.Channel.GetMessagesAsync(x + 1).Flatten();
                await Context.Channel.DeleteMessagesAsync(messagesToDelete);
                var embed = new EmbedBuilder();
                var guild = Context.Guild as SocketGuild;
                embed.WithColor(new Color(0x4d006d));
                embed.Title = string.Format(" ");
                embed.Description = string.Format("{0} messages deleted!", x);
                await ReplyAndDeleteAsync("", false, embed.Build()).ConfigureAwait(false);
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(0x4d006d));
                embed.Title = string.Format(" ");
                embed.Description = string.Format("you cannot delete more than 1000 messages");
                await ReplyAndDeleteAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("Ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null) throw new ArgumentException("You must mention a user");
            var guild = Context.Guild as SocketGuild;
            await guild.AddBanAsync(user, 7, $"Banned by {Context.User}").ConfigureAwait(false);
        }

        [Command("Kick")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(SocketGuildUser user, [Remainder] string reason)
        {
            if (user == null) throw new ArgumentException("You must mention a user");
            await user.KickAsync().ConfigureAwait(false);
        }

        [Command("mute", RunMode = RunMode.Async)]
        [Alias("Mute", "m")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Mute(SocketGuildUser user)
        {
            var alterAuthor = Context.User as IGuildUser;
            var alterUser = user as IGuildUser;
            var adminRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Admiral");
            var modRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Staff");

            var adminCheck = alterUser.RoleIds.Contains(adminRole.Id);
            var staffCheck = alterUser.RoleIds.Contains(modRole.Id);
            try
            {
                if (adminCheck == false && staffCheck == false)
                {
                    await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
                    var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                    try
                    {
                        await user.AddRoleAsync(muteRole);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to add role");
                    }
                    try
                    {
                        IMessage[] msgs;
                        IMessage lastMessage = null;
                        msgs = (await Context.Channel.GetMessagesAsync(50).Flatten()).Where(m => m.Author.Id == user.Id).Take(50).ToArray();
                        lastMessage = msgs[msgs.Length - 1];

                        var bulkDeletable = new List<IMessage>();
                        foreach (var x in msgs)
                        {
                            bulkDeletable.Add(x);
                        }
                        bulkDeletable.Add(Context.Message as IMessage);

                        await Task.WhenAll(Task.Delay(1000), Context.Channel.DeleteMessagesAsync(bulkDeletable)).ConfigureAwait(false);
                    }
                    catch
                    {
                        Console.Write($"{DateTime.Now} | Mute - No messages to delete\n");
                    }
                }
                else { Console.WriteLine("Staff or admin check failed on mute"); }
            }
            catch
            {
                Console.WriteLine($"{DateTime.Now} - Mute failed on {user.Username}#{user.Discriminator}");
            }
        }

        [Command("unmute", RunMode = RunMode.Async)]
        [Alias("Unmute", "unm")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Unmute(SocketGuildUser user)
        {
            var alterAuthor = Context.User as IGuildUser;
            var alterUser = user as IGuildUser;
            var adminRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Admiral");
            var modRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Staff");
            var adminCheck = alterUser.RoleIds.Contains(adminRole.Id);
            var staffCheck = alterUser.RoleIds.Contains(modRole.Id);
            try
            {
                if (adminCheck == false && staffCheck == false && user.IsBot != true)
                {
                    await user.ModifyAsync(x => x.Mute = false).ConfigureAwait(false);
                    var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                    await user.RemoveRoleAsync(muteRole);
                    Console.WriteLine($"{DateTime.Now} | Unmuted {user.Username}#{user.Discriminator}");
                }
                else { Console.WriteLine("Staff or admin check failed on unmute"); }
            }
            catch
            {
                Console.WriteLine($"{DateTime.Now} | Unmute failed on {user.Username}#{user.Discriminator}");
            }
        }

    }
}
