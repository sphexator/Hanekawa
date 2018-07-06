using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Administration;
using Jibril.Services.Entities;

namespace Jibril.Modules.Administration
{
    public class Administration : InteractiveBase
    {
        private readonly MuteService _muteService;
        private readonly WarnService _warnService;

        public Administration(MuteService muteService, WarnService warnService)
        {
            _muteService = muteService;
            _warnService = warnService;
        }

        [Command("ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(SocketGuildUser user)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            if (Context.User.Id != user.Guild.OwnerId && user.Roles.Select(r => r.Position).Max() >=
                ((SocketGuildUser) Context.User).Roles.Select(r => r.Position)
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
                ((SocketGuildUser) Context.User).Roles.Select(r => r.Position)
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
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task PruneAsync(uint x = 1, IGuildUser user = null)
        {
            if (x > 1000) x = 1000;
            var messages = new List<IMessage>();
            if (user == null)
            {
                messages = (await Context.Channel.GetMessagesAsync((int) x).FlattenAsync()).ToList();
            }
            else
            {
                var msgs = (await Context.Channel.GetMessagesAsync((int) x).FlattenAsync())
                    .Where(m => m.Author.Id == user.Id)
                    .Take((int) x).ToArray();
                messages.AddRange(msgs);
            }

            messages.Add(Context.Message);
            await Task.WhenAll(Task.Delay(1000), (Context.Channel as ITextChannel)?.DeleteMessagesAsync(messages))
                .ConfigureAwait(false);
            var embed = new EmbedBuilder().Reply($"{messages.Count} messages deleted!", Color.Green.RawValue);
            await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(15));
        }

        [Command("mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(SocketGuildUser user, uint timer = 1440, string reason = null)
        {
            await Context.Message.DeleteAsync();
            var mute = _muteService.TimedMute(user, (SocketGuildUser) Context.User, TimeSpan.FromMinutes(timer));
            var warn = _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Mute);
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
        public async Task WarnUserAsync(SocketGuildUser user, [Remainder] string reason = "No Reason Provided")
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
                var actionCase = await db.ModLogs.FindAsync(id);
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
                    Color = embed.Color
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
                    x.Value = $"{Context.User.Username}";
                    x.IsInline = true;
                });
                try
                {
                    var length = embed.Fields.First(x => x.Name == "Length");
                    updEmbed.AddField(x =>
                    {
                        x.Name = length.Name;
                        x.Value = length.Value;
                        x.IsInline = length.IsInline;
                    });
                }
                catch
                {
                    /*ignore*/
                }

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