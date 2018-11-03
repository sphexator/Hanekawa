using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Hanekawa.Services.Administration;
using Hanekawa.Services.AutoModerator;

namespace Hanekawa.Modules.Administration
{
    public class Administration : InteractiveBase
    {
        private readonly MuteService _muteService;
        private readonly NudeScoreService _nudeScore;
        private readonly WarnService _warnService;

        public Administration(MuteService muteService, WarnService warnService, NudeScoreService nudeScore)
        {
            _muteService = muteService;
            _warnService = warnService;
            _nudeScore = nudeScore;
        }

        [Command("ban", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [Summary("Bans a user")]
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
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [Summary("Kicks a user")]
        public async Task KickAsync(IGuildUser user)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            if (Context.User.Id != user.Guild.OwnerId && ((SocketGuildUser
                    ) user).Roles.Select(r => r.Position).Max() >=
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
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [Summary("Prunes X messages, user specific is optional")]
        public async Task PruneAsync(int x = 5, IGuildUser user = null)
        {
            if (x > 1000) x = 1000;
            if (user == null)
            {
                var channel = Context.Channel as ITextChannel;
                var msgs = await channel.GetMessagesAsync(x + 1).FlattenAsync().ConfigureAwait(false);
                await channel.DeleteMessagesAsync(msgs).ConfigureAwait(false);
                var embed = new EmbedBuilder().Reply($"{msgs.Count()} messages deleted!", Color.Green.RawValue);
                await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(30));
            }
            else
            {
                var msgs = (await Context.Channel.GetMessagesAsync(x + 1).FlattenAsync())
                    .Where(m => m.Author.Id == user.Id)
                    .Take(x);
                var channel = Context.Channel as ITextChannel;
                await channel.DeleteMessagesAsync(msgs).ConfigureAwait(false);
                var embed = new EmbedBuilder().Reply($"{x} messages deleted!", Color.Green.RawValue);
                await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(30));
            }
        }

        [Command("softban", RunMode = RunMode.Async)]
        [Alias("sb")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Summary("In the last 1000 messages, deletes the messages user has sent & mutes")]
        public async Task Softban(SocketGuildUser user)
        {
            if (Context.User.Id != user.Guild.OwnerId && user.Roles.Select(r => r.Position).Max() >=
                Context.Guild.Roles.Select(r => r.Position).Max())
            {
                await ReplyAndDeleteAsync("", false,
                    new EmbedBuilder()
                        .Reply($"{Context.User.Mention}, you can't mute someone with same or higher role then you.",
                            Color.Red.RawValue).Build(), TimeSpan.FromSeconds(15));
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
                var msgs = (await Context.Channel.GetMessagesAsync(50).FlattenAsync())
                    .Where(m => m.Author.Id == user.Id)
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
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Priority(1)]
        [Summary("mutes a user for a duration specified in minutes (max 1440)")]
        public async Task MuteAsync(SocketGuildUser user, uint minutes = 1440, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            var mute = _muteService.TimedMute(user, (SocketGuildUser) Context.User, TimeSpan.FromMinutes(minutes));
            var warn = _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Mute,
                TimeSpan.FromMinutes(minutes));
            await Task.WhenAll(mute, warn);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"Muted {user.Mention}", Color.Green.RawValue).Build(),
                TimeSpan.FromSeconds(15));
        }

        [Command("mute", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Mutes a user for 12hrs")]
        public async Task MuteAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            var mute = _muteService.TimedMute(user, (SocketGuildUser) Context.User, TimeSpan.FromMinutes(1440));
            var warn = _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Mute,
                TimeSpan.FromMinutes(1440));
            await Task.WhenAll(mute, warn);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"Muted {user.Mention}", Color.Green.RawValue).Build(),
                TimeSpan.FromSeconds(15));
        }

        [Command("mute", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Mutes a user for a duration (default 12hrs) with the use of 1s 2h 1d")]
        [Priority(2)]
        public async Task MuteAsync(SocketGuildUser user, TimeSpan? timer = null, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            if (!timer.HasValue) timer = TimeSpan.FromHours(12);
            if (timer.Value > TimeSpan.FromDays(1)) timer = TimeSpan.FromDays(1);
            if (timer.Value < TimeSpan.FromMinutes(10)) timer = TimeSpan.FromMinutes(10);
            var mute = _muteService.TimedMute(user, (SocketGuildUser) Context.User, timer.Value);
            var warn = _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Mute,
                timer.Value);
            await Task.WhenAll(mute, warn);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"Muted {user.Mention}", Color.Green.RawValue).Build(),
                TimeSpan.FromSeconds(15));
        }

        [Command("unmute", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Unmutes a user")]
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
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Summary("Sends a warning to a user, bot dms them the warning.")]
        public async Task WarnUserAsync(SocketGuildUser user, [Remainder] string reason = "I made this :)")
        {
            await Context.Message.DeleteAsync();
            var msgs = (await Context.Channel.GetMessagesAsync().FlattenAsync()).Where(m => m.Author.Id == user.Id)
                .Take(100).ToList();
            await _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Warning, msgs);
            await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"Warned {user.Mention}").Build());
        }

        [Command("warnlog", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Summary("Pulls up warnlog and admin profile of a user.")]
        public async Task WarnlogAsync(SocketGuildUser user, WarnLogType type = WarnLogType.Simple)
        {
            if (type == WarnLogType.Simple)
            {
                var log = await _warnService.GetSimpleWarnlogAsync(user);
                await Context.Channel.SendEmbedAsync(log);
            }
            else
            {
                var pages = await _warnService.GetFullWarnlogAsync(user);
                var paginator = new PaginatedMessage
                {
                    Color = Color.Purple,
                    Pages = pages,
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = user.GetAvatar(),
                        Name = $"Full warn log for {user.Username}"
                    },
                    Options = new PaginatedAppearanceOptions
                    {
                        First = new Emoji("⏮"),
                        Back = new Emoji("◀"),
                        Next = new Emoji("▶"),
                        Last = new Emoji("⏭"),
                        Stop = null,
                        Jump = null,
                        Info = null
                    }
                };
                await PagedReplyAsync(paginator);
            }
        }

        [Command("toxicity", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Shows toxicity values of users if server has toxicity enabled channels")]
        public async Task ViewToxicity(SocketGuildUser user = null, ITextChannel channel = null,
            WarnToxicityType type = WarnToxicityType.Single)
        {
            if (channel == null && user == null)
            {
                var pages = _nudeScore.GetGuildTopScore(Context.Guild);
                if (pages == null || !pages.Any())
                {
                    await ReplyAsync(null, false, new EmbedBuilder().Reply("No values", Color.Red.RawValue).Build());
                    return;
                }

                var paginator = new PaginatedMessage
                {
                    Color = Color.Purple,
                    Pages = pages,
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = Context.Guild.IconUrl,
                        Name = $"Toxicity values in {Context.Guild.Name}"
                    },
                    Options = new PaginatedAppearanceOptions
                    {
                        First = new Emoji("⏮"),
                        Back = new Emoji("◀"),
                        Next = new Emoji("▶"),
                        Last = new Emoji("⏭"),
                        Stop = null,
                        Jump = null,
                        Info = null
                    }
                };
                await PagedReplyAsync(paginator);
                return;
            }

            if (user == null)
            {
                var page = _nudeScore.GetChannelTopScores(channel);
                if (page == null)
                {
                    await ReplyAsync(null, false, new EmbedBuilder().Reply("No values", Color.Red.RawValue).Build());
                    return;
                }

                await ReplyAsync(null, false, new EmbedBuilder().Reply(page).Build());
                return;
            }

            if (channel != null && type == WarnToxicityType.Single)
            {
                var page = _nudeScore.GetSingleScore(channel, user);
                if (page == 0)
                {
                    await ReplyAsync(null, false, new EmbedBuilder().Reply("No values", Color.Red.RawValue).Build());
                    return;
                }

                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Toxicity score in {channel.Mention}: {page}").Build());
            }
            else
            {
                var pages = _nudeScore.GetAllScores(user);
                if (pages == null || !pages.Any())
                {
                    await ReplyAsync(null, false, new EmbedBuilder().Reply("No values", Color.Red.RawValue).Build());
                    return;
                }

                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = user.GetAvatar(),
                        Name = $"Toxicity values for {user.GetName()} in {Context.Guild.Name}"
                    },
                    Color = Color.Purple,
                    Description = pages
                };
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("reason", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Inputs reason for moderation log entry")]
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