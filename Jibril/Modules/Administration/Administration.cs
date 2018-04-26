using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Administration.Services;
using Jibril.Preconditions;
using Jibril.Services.Common;
using Jibril.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Administration
{
    public class Administration : InteractiveBase
    {
        private readonly TimedMuteService _muteService;

        public Administration(TimedMuteService muteService)
        {
            _muteService = muteService;
        }

        [Command("prune", RunMode = RunMode.Async)]
        [Alias("Prune")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireRole(339371670311796736)]
        public async Task ClearMessage([Remainder] int x = 0)
        {
            if (x <= 2000)
            {
                var channel = Context.Channel as ITextChannel;
                var messagesToDelete = await Context.Channel.GetMessagesAsync(x + 1).FlattenAsync();
                await channel.DeleteMessagesAsync(messagesToDelete);
                var embed = EmbedGenerator.DefaultEmbed($"{messagesToDelete.Count()} messages deleted!",
                    Colours.OkColour);
                await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(15)).ConfigureAwait(false);
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed("you cannot delete more than 1000 messages",
                    Colours.FailColour);
                await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
        }
        [Command("Ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireRole(339371670311796736)]
        public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = "No Reason provided")
        {
            if (user == null) throw new ArgumentException("You must mention a user");
            await Context.Message.DeleteAsync();
            if (Context.User.Id != user.Guild.OwnerId && (user.Roles.Select(r => r.Position).Max() >= ((SocketGuildUser)Context.User).Roles.Select(r => r.Position).Max()))
            {
                var failEmbed = EmbedGenerator.DefaultEmbed(
                    $"{Context.User.Mention}, you can't ban someone with same or higher role then you.",
                    Colours.FailColour);
                await ReplyAndDeleteAsync("", false, failEmbed.Build(), TimeSpan.FromSeconds(15)).ConfigureAwait(false);
                return;
            }

            var guild = Context.Guild;
            var embed = EmbedGenerator.DefaultEmbed($"Banned {user.Mention} from {Context.Guild.Name}",
                Colours.OkColour);

            await guild.AddBanAsync(user, 7, $"{Context.User}").ConfigureAwait(false);
            await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        [Command("Kick", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireRole(339371670311796736)]
        public async Task KickAsync(SocketGuildUser user, [Remainder] string reason)
        {
            if (user == null) throw new ArgumentException("You must mention a user");
            await Context.Message.DeleteAsync();
            if (Context.User.Id != user.Guild.OwnerId && user.Roles.Select(r => r.Position).Max() >=
                Context.Guild.Roles.Select(r => r.Position).Max())
            {
                var failEmbed = EmbedGenerator.DefaultEmbed(
                    $"{Context.User.Mention}, you can't kick someone with same or higher role then you.",
                    Colours.FailColour);
                await ReplyAndDeleteAsync("", false, failEmbed.Build(), TimeSpan.FromSeconds(15));
                return;
            }
            var embed = EmbedGenerator.DefaultEmbed($"Kicked {user.Username} from {Context.Guild.Name}",
                Colours.OkColour);
            await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            await user.KickAsync().ConfigureAwait(false);
        }
        [Command("mute", RunMode = RunMode.Async)]
        [Alias("m")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireRole(339371670311796736)]
        public async Task DefaultMute(SocketGuildUser user)
        {
            try
            {
                await Context.Message.DeleteAsync();
                await _muteService.TimedMute(user, TimeSpan.FromMinutes(1440));
                var confirmEmbed = EmbedGenerator.DefaultEmbed($"{Context.User} Muted {user.Mention}", Colours.OkColour);
                await ReplyAndDeleteAsync("", false, confirmEmbed.Build(), TimeSpan.FromSeconds(5));

                await MuteLogResponse(Context.Guild, Context.User, user);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        [Command("mute", RunMode = RunMode.Async)]
        [Alias("m")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireRole(339371670311796736)]
        public async Task TimedMute(int minutes, SocketGuildUser user)
        {
            if (minutes < 1 || minutes > 1440) return;
            try
            {
                await Context.Message.DeleteAsync();
                await _muteService.TimedMute(user, TimeSpan.FromMinutes(minutes));
                var confirmEmbed = EmbedGenerator.DefaultEmbed($"{Context.User} Muted {user.Mention}", Colours.OkColour);
                await ReplyAndDeleteAsync("", false, confirmEmbed.Build(), TimeSpan.FromSeconds(5));

                await MuteLogResponse(Context.Guild, Context.User, user, minutes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        [Command("unmute", RunMode = RunMode.Async)]
        [Alias("Unmute", "unm")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireRole(339371670311796736)]
        public async Task Unmute(SocketGuildUser user)
        {
            await Context.Message.DeleteAsync();
            await _muteService.UnmuteUser(user);

            var confirmEmbed = EmbedGenerator.DefaultEmbed($"{Context.User} unmuted {user.Mention}", Colours.OkColour);
            await ReplyAndDeleteAsync("", false, confirmEmbed.Build(), TimeSpan.FromSeconds(5));
        }

        [Command("softban", RunMode = RunMode.Async)]
        [Alias("sb")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireRole(339371670311796736)]
        public async Task Softban(SocketGuildUser user)
        {
            if (Context.User.Id != user.Guild.OwnerId && user.Roles.Select(r => r.Position).Max() >=
                Context.Guild.Roles.Select(r => r.Position).Max())
            {
                var failEmbed = EmbedGenerator.DefaultEmbed(
                    $"{Context.User.Mention}, you can't mute someone with same or higher role then you.",
                    Colours.FailColour);
                await ReplyAndDeleteAsync("", false, failEmbed.Build(), TimeSpan.FromSeconds(15));
                return;
            }

            var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
            await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            try
            {
                await user.AddRoleAsync(muteRole);
            }
            catch
            {
                // Didn't find role
            }

            try
            {
                var msgs = (await Context.Channel.GetMessagesAsync(50).FlattenAsync()).Where(m => m.Author.Id == user.Id)
                    .Take(50).ToArray();

                var bulkDeletable = new List<IMessage>();
                foreach (var x in msgs)
                    bulkDeletable.Add(x);
                bulkDeletable.Add(Context.Message);

                var channel = Context.Channel as ITextChannel;
                await Task.WhenAll(Task.Delay(1000), channel.DeleteMessagesAsync(bulkDeletable)).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }

        private async Task MuteLogResponse(SocketGuild guild, IUser user, IUser mutedUser, int length = 1440)
        {
            var time = DateTime.Now;
            AdminDb.AddActionCase(user, time);
            var caseid = AdminDb.GetActionCaseID(time);

            var author = new EmbedAuthorBuilder
            {
                IconUrl = mutedUser.GetAvatarUrl(),
                Name = $"Case {caseid[0]} | {ActionType.Gagged} | {mutedUser.Username}#{mutedUser.DiscriminatorValue}"
            };
            var footer = new EmbedFooterBuilder
            {
                Text = $"ID:{mutedUser.Id} | {DateTime.UtcNow}"
            };
            var embed = new EmbedBuilder
            {
                Color = new Color(Colours.FailColour),
                Author = author,
                Footer = footer
            };
            embed.AddField(x =>
            {
                x.Name = "User";
                x.Value = $"{mutedUser.Mention}";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Moderator";
                x.Value = $"{Context.User.Username}";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Length";
                x.Value = $"{length}";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Reason";
                x.Value = "N/A";
                x.IsInline = true;
            });

            var log = guild.GetTextChannel(339381104534355970);
            var msg = await log.SendMessageAsync("", false, embed.Build());
            CaseNumberGenerator.UpdateCase(msg.Id.ToString(), caseid[0]);
        }
    }
}