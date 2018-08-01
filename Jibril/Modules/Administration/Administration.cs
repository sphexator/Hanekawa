using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Administration;
using Jibril.Services.Entities;
using Jibril.Services.Level;

namespace Jibril.Modules.Administration
{
    public class Administration : InteractiveBase
    {
        private readonly MuteService _muteService;
        private readonly WarnService _warnService;
        private readonly LevelingService _levelingService;

        public Administration(MuteService muteService, WarnService warnService, LevelingService levelingService)
        {
            _muteService = muteService;
            _warnService = warnService;
            _levelingService = levelingService;
        }

        [Command("exp", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ExpEventAsync(uint multiplier, uint duration = 1440)
        {
            try
            {
                var after = TimeSpan.FromMinutes(duration);
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply(
                        $"Wanna activate a exp event with multiplier of {multiplier} for {after.Humanize()} ({duration} minutes) ? (y/n)",
                        Color.DarkPurple.RawValue).Build());
                var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                if (response.Content.ToLower() != "y") return;

                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Do you want to announce the event? (y/n)",
                        Color.DarkPurple.RawValue).Build());
                var announceResp = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                if (announceResp.Content.ToLower() == "y")
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Okay, I'll let you announce it...",
                            Color.Green.RawValue).Build());
                    await _levelingService.AddExpMultiplierAsync(Context.Guild, multiplier, after);
                }
                else
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Announcing event into designated channel.",
                            Color.Green.RawValue).Build());
                    await _levelingService.AddExpMultiplierAsync(Context.Guild, multiplier, after, true, Context.Channel as SocketTextChannel);
                }
            }
            catch
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Exp event setup aborted.",
                        Color.Red.RawValue).Build());
            }
        }

        [Command("ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(SocketGuildUser user)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            if (Context.User.Id != user.Guild.OwnerId && user.Roles.Select(r => r.Position).Max() >=
                ((SocketGuildUser)Context.User).Roles.Select(r => r.Position)
                .Max())
            {
                var fembed = new EmbedBuilder().Reply(
                    $"{Context.User.Mention}, can't ban someone that's equal or more power than you, BAKA!",
                    Color.Red.RawValue);
                await ReplyAndDeleteAsync(null, false, fembed.Build(), TimeSpan.FromSeconds(15));
                return;
            }

            await Context.Guild.AddBanAsync(user, 7, $"{Context.User.Id}").ConfigureAwait(false);
            var embed = new EmbedBuilder().Reply($"Banned {user.Mention} from {Context.Guild.Name}.",
                Color.Green.RawValue);
            await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(15));
        }

        [Command("kick", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task KickAsync(IGuildUser user)
        {

            await Context.Message.DeleteAsync().ConfigureAwait(false);
            if (Context.User.Id != user.Guild.OwnerId && ((SocketGuildUser
            )user).Roles.Select(r => r.Position).Max() >=
                ((SocketGuildUser)Context.User).Roles.Select(r => r.Position)
                .Max())
            {
                var fembed = new EmbedBuilder().Reply(
                    $"{Context.User.Mention}, can't kick someone that's equal or more power than you, BAKA!",
                    Color.Red.RawValue);
                await ReplyAndDeleteAsync(null, false, fembed.Build(), TimeSpan.FromSeconds(15));
                return;
            }

            await user.KickAsync($"{Context.User.Id}").ConfigureAwait(false);
            var embed = new EmbedBuilder().Reply($"Kicked {user.Mention} from {Context.Guild.Name}.",
                Color.Green.RawValue);
            await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(15));
        }

        [Command("prune", RunMode = RunMode.Async)]
        [Alias("clear")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task PruneAsync(int x = 5, IGuildUser user = null)
        {
            if (x > 1000) x = 1000;
            if (user == null)
            {
                var msgs = await Context.Channel.GetMessagesAsync(x + 1).FlattenAsync();
                var channel = Context.Channel as ITextChannel;
                await channel.DeleteMessagesAsync(msgs).ConfigureAwait(false);
                var embed = new EmbedBuilder().Reply($"{msgs.Count()} messages deleted!", Color.Green.RawValue);
                await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(15));
            }
            else
            {
                var msgs = (await Context.Channel.GetMessagesAsync(x + 1).FlattenAsync())
                    .Where(m => m.Author.Id == user.Id)
                    .Take(x);
                var channel = Context.Channel as ITextChannel;
                await channel.DeleteMessagesAsync(msgs).ConfigureAwait(false);
                var embed = new EmbedBuilder().Reply($"{msgs.Count()} messages deleted!", Color.Green.RawValue);
                await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(15));
            }
        }

        [Command("softban", RunMode = RunMode.Async)]
        [Alias("sb")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Softban(SocketGuildUser user)
        {
            if (Context.User.Id != user.Guild.OwnerId && user.Roles.Select(r => r.Position).Max() >=
                Context.Guild.Roles.Select(r => r.Position).Max())
            {
                await ReplyAndDeleteAsync("", false, new EmbedBuilder().Reply($"{Context.User.Mention}, you can't mute someone with same or higher role then you.", Color.Red.RawValue).Build(), TimeSpan.FromSeconds(15));
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

                var bulkDeletable = msgs.ToList();
                bulkDeletable.Add(Context.Message);

                var channel = Context.Channel as ITextChannel;
                await Task.WhenAll(Task.Delay(1000), channel.DeleteMessagesAsync(bulkDeletable)).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }

        [Command("mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(SocketGuildUser user, uint timer = 1440,[Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            var mute = _muteService.TimedMute(user, (SocketGuildUser)Context.User, TimeSpan.FromMinutes(timer));
            var warn = _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Mute, TimeSpan.FromMinutes(timer));
            await Task.WhenAll(mute, warn);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"Muted {user.Mention}", Color.Green.RawValue).Build(),
                TimeSpan.FromSeconds(15));
        }

        [Command("unmute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnmuteAsync(SocketGuildUser user)
        {
            await Context.Message.DeleteAsync();
            await _muteService.UnmuteUser(user);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"Unmuted {user.Mention}", Color.Green.RawValue).Build(),
                TimeSpan.FromSeconds(15));
        }

        [Command("warn", RunMode = RunMode.Async)]
        [Alias("warning")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task WarnUserAsync(SocketGuildUser user, [Remainder] string reason = "I made this :)")
        {
            await Context.Message.DeleteAsync();
            var msgs = (await Context.Channel.GetMessagesAsync().FlattenAsync()).Where(m => m.Author.Id == user.Id)
                .Take(100).ToList();
            await _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Warning, msgs);
            await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"Warned {user.Mention}").Build());
        }

        [Command("warnlog", RunMode = RunMode.Async)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task WarnlogAsync(SocketGuildUser user)
        {
            var log = await _warnService.Warnlog(user);
            await Context.Channel.SendEmbedAsync(log);
        }

        [Command("reason", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ApplyReason(uint id, [Remainder] string reason)
        {
            using (var db = new DbService())
            {
                var actionCase = await db.ModLogs.FindAsync(id, Context.Guild.Id);
                var updMsg = await Context.Channel.GetMessageAsync(actionCase.MessageId) as IUserMessage;
                var embed = updMsg?.Embeds.First().ToEmbedBuilder();
                if (embed == null) return;
                var author = new EmbedAuthorBuilder
                {
                    Name = embed.Author?.Name,
                    Url = embed.Author?.IconUrl
                };
                var footer = new EmbedFooterBuilder
                {
                    Text = embed.Footer?.Text
                };
                var updEmbed = new EmbedBuilder
                {
                    Author = author,
                    Footer = footer,
                    Color = embed.Color,
                    Timestamp = embed.Timestamp
                };
                var userField = embed.Fields.FirstOrDefault(x => x.Name == "User");
                updEmbed.AddField(x =>
                {
                    if (userField == null) return;
                    x.Name = userField.Name;
                    x.Value = userField.Value;
                    x.IsInline = userField.IsInline;
                });
                updEmbed.AddField(x =>
                {
                    x.Name = "Moderator";
                    x.Value = $"{(Context.User as SocketGuildUser).GetName()}";
                    x.IsInline = true;
                });
                try
                {
                    var length = embed.Fields.First(x => x.Name == "Duration");
                    updEmbed.AddField(x =>
                    {
                        x.Name = length.Name;
                        x.Value = length.Value;
                        x.IsInline = length.IsInline;
                    });
                }
                catch{/*ignore*/}

                updEmbed.AddField(x =>
                {
                    x.Name = "Reason";
                    x.Value = reason != null ? $"{reason}" : "No Reason Provided";
                    x.IsInline = true;
                });
                await Context.Message.DeleteAsync();
                await updMsg.ModifyAsync(m => m.Embed = updEmbed.Build());
                actionCase.Response = reason;
                actionCase.ModId = Context.User.Id;
                await db.SaveChangesAsync();
            }
        }
    }
}