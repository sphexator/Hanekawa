using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Administration;
using Hanekawa.Services.AutoModerator;
using Quartz.Util;

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

        [Name("Ban")]
        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Bans a user")]
        [Remarks("h.ban @bob#0000")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(SocketGuildUser user)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            if (Context.Guild.GetUser(Context.Client.CurrentUser.Id).HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault("Cannot ban someone that's higher than me in hierarchy.",
                        Color.Red.RawValue).Build(), TimeSpan.FromSeconds(20));
                return;
            }

            if ((Context.User as SocketGuildUser).HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault(
                        $"{Context.User.Mention}, can't ban someone that's equal or more power than you.",
                        Color.Red.RawValue).Build(), TimeSpan.FromSeconds(20));
                return;
            }

            await Context.Guild.AddBanAsync(user, 7, $"{Context.User.Id}");
            await ReplyAndDeleteAsync(null, false, new EmbedBuilder().CreateDefault(
                $"Banned {user.Mention} from {Context.Guild.Name}.",
                Color.Green.RawValue).Build(), TimeSpan.FromSeconds(20));
        }

        [Name("Kick")]
        [Command("kick", RunMode = RunMode.Async)]
        [Summary("Kicks a user")]
        [Remarks("h.kick @bob#0000")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(SocketGuildUser user)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            if (Context.Guild.GetUser(Context.Client.CurrentUser.Id).HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault("Cannot kick someone that's higher than me in hierarchy.",
                        Color.Red.RawValue).Build(), TimeSpan.FromSeconds(20));
                return;
            }

            if ((Context.User as SocketGuildUser).HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault(
                        $"{Context.User.Mention}, can't kick someone that's equal or more power than you.",
                        Color.Red.RawValue).Build(), TimeSpan.FromSeconds(20));
                return;
            }

            await user.KickAsync($"{Context.User.Id}").ConfigureAwait(false);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().CreateDefault($"Kicked {user.Mention} from {Context.Guild.Name}.",
                    Color.Green.RawValue).Build(), TimeSpan.FromSeconds(15));
        }

        [Name("Prune")]
        [Command("prune", RunMode = RunMode.Async)]
        [Summary("Prunes X messages, user specific is optional")]
        [Remarks("h.prune 500")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PruneAsync(int x = 5, IGuildUser user = null)
        {
            if (x > 1000) x = 1000;
            if (user == null)
            {
                if (!(Context.Channel is ITextChannel channel)) return;
                var msgs = (await channel.GetMessagesAsync(x + 1).FlattenAsync()).Where(m =>
                    m.Timestamp >= DateTimeOffset.UtcNow - TimeSpan.FromDays(11));
                var messages = msgs.ToList();
                await channel.DeleteMessagesAsync(messages).ConfigureAwait(false);
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault($"{messages.Count} messages deleted!", Color.Green.RawValue)
                        .Build(), TimeSpan.FromSeconds(10));
            }
            else
            {
                var msgs = (await Context.Channel.GetMessagesAsync(x + 1).FlattenAsync())
                    .Where(m => m.Author.Id == user.Id && m.Timestamp >= DateTimeOffset.UtcNow - TimeSpan.FromDays(11))
                    .Take(x);
                if (!(Context.Channel is ITextChannel channel)) return;
                await channel.DeleteMessagesAsync(msgs).ConfigureAwait(false);
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault($"{x} messages deleted!", Color.Green.RawValue).Build(),
                    TimeSpan.FromSeconds(10));
            }
        }

        [Name("Soft ban")]
        [Command("softban", RunMode = RunMode.Async)]
        [Alias("sb")]
        [Summary("In the last 1000 messages, deletes the messages user has sent and mutes")]
        [Remarks("h.softban @bob#0000")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Softban(SocketGuildUser user)
        {
            if (Context.User.Id != user.Guild.OwnerId && user.Roles.Select(r => r.Position).Max() >=
                Context.Guild.Roles.Select(r => r.Position).Max())
            {
                await ReplyAndDeleteAsync("", false,
                    new EmbedBuilder()
                        .CreateDefault(
                            $"{Context.User.Mention}, you can't mute someone with same or higher role then you.",
                            Color.Red.RawValue).Build(), TimeSpan.FromSeconds(15));
                return;
            }

            var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
            await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            try
            {
                await user.TryAddRoleAsync(muteRole);
            }
            catch
            {
                // Didn't find role
            }

            try
            {
                var msgs = (await Context.Channel.GetMessagesAsync(50).FlattenAsync())
                    .Where(m => m.Author.Id == user.Id && m.Timestamp >= DateTimeOffset.UtcNow - TimeSpan.FromDays(11))
                    .Take(50).ToList();
                msgs.Add(Context.Message);
                if (!(Context.Channel is ITextChannel channel)) return;
                await Task.WhenAll(Task.Delay(1000), channel.DeleteMessagesAsync(msgs)).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }

        [Name("Mute")]
        [Command("mute", RunMode = RunMode.Async)]
        [Summary("Mutes a user for a duration specified in minutes (max 1440)")]
        [Remarks("h.mute @bob#0000 400")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Priority(1)]
        public async Task MuteAsync(SocketGuildUser user, uint minutes = 1440, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            var mute = _muteService.TimedMute(user, (SocketGuildUser) Context.User, TimeSpan.FromMinutes(minutes));
            var warn = _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Mute,
                TimeSpan.FromMinutes(minutes));
            await Task.WhenAll(mute, warn);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().CreateDefault($"Muted {user.Mention}", Color.Green.RawValue).Build(),
                TimeSpan.FromSeconds(15));
        }

        [Name("Mute")]
        [Command("mute", RunMode = RunMode.Async)]
        [Summary("Mutes a user for 12hrs")]
        [Remarks("h.mute @bob#0000")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            var mute = _muteService.TimedMute(user, (SocketGuildUser) Context.User, TimeSpan.FromMinutes(1440));
            var warn = _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Mute,
                TimeSpan.FromMinutes(1440));
            await Task.WhenAll(mute, warn);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().CreateDefault($"Muted {user.Mention}", Color.Green.RawValue).Build(),
                TimeSpan.FromSeconds(15));
        }

        [Name("Mute")]
        [Command("mute", RunMode = RunMode.Async)]
        [Summary("Mutes a user for a duration (default 12hrs) with the use of 1s 2h 1d")]
        [Remarks("h.mute @bob#0000 12h")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
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
                new EmbedBuilder().CreateDefault($"Muted {user.Mention}", Color.Green.RawValue).Build(),
                TimeSpan.FromSeconds(15));
        }

        [Name("Unmute")]
        [Command("unmute", RunMode = RunMode.Async)]
        [Summary("Unmutes a user")]
        [Remarks("h.unmute @bob#0000")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnmuteAsync(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                await _muteService.UnmuteUser(db, user);
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault($"Unmuted {user.Mention}", Color.Green.RawValue).Build(),
                    TimeSpan.FromSeconds(15));
            }
        }

        [Name("Warn")]
        [Command("warn", RunMode = RunMode.Async)]
        [Alias("warning")]
        [Summary("Sends a warning to a user, bot dms them the warning.")]
        [Remarks("h.warn @bob#0000")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task WarnUserAsync(SocketGuildUser user, [Remainder] string reason = "I made this :)")
        {
            await Context.Message.DeleteAsync();
            var msgs = (await Context.Channel.GetMessagesAsync().FlattenAsync()).Where(m => m.Author.Id == user.Id)
                .Take(100).ToList();
            await _warnService.AddWarning(user, Context.User, DateTime.UtcNow, reason, WarnReason.Warning, msgs);
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().CreateDefault($"Warned {user.Mention}", Context.Guild.Id).Build());
        }

        [Name("Warnlog")]
        [Command("warnlog", RunMode = RunMode.Async)]
        [Summary("Pulls up warnlog and admin profile of a user.")]
        [Remarks("h.warnlog @bob#0000")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task WarnlogAsync(SocketGuildUser user, WarnLogType type = WarnLogType.Simple)
        {
            if (type == WarnLogType.Simple)
            {
                var log = await _warnService.GetSimpleWarnlogAsync(user);
                await Context.ReplyAsync(log);
            }
            else
            {
                var pages = await _warnService.GetFullWarnlogAsync(user);
                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, user,
                    $"Full warn log for {user.Username}"));
            }
        }

        [Name("Toxicity")]
        [Command("toxicity", RunMode = RunMode.Async)]
        [Summary("Shows toxicity values of users if server has toxicity enabled channels")]
        [Remarks("h.toxicity @bob#0000")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Priority(1)]
        public async Task ViewToxicity(SocketGuildUser user = null)
        {
            if (user == null)
            {
                var pages = _nudeScore.GetGuildTopScore(Context.Guild).ToList();
                if (pages.Count == 0)
                {
                    await Context.ReplyAsync("No values", Color.Red.RawValue);
                    return;
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Toxicity values in {Context.Guild.Name}"));
            }
            else
            {
                var toxicity = _nudeScore.GetAllScores(user);
                if (toxicity == null || toxicity.IsNullOrWhiteSpace())
                {
                    await Context.ReplyAsync("No values", Color.Red.RawValue);
                    return;
                }

                await Context.ReplyAsync(toxicity);
            }
        }

        [Name("Toxicity")]
        [Command("toxicity", RunMode = RunMode.Async)]
        [Summary("Shows toxicity values of users in a channel if server has toxicity enabled channels")]
        [Remarks("h.toxicity #general")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ViewToxicity(ITextChannel channel = null)
        {
            if (channel == null)
            {
                var pages = _nudeScore.GetGuildTopScore(Context.Guild).ToList();
                if (pages.Count == 0)
                {
                    await Context.ReplyAsync("No values", Color.Red.RawValue);
                    return;
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Toxicity values in {Context.Guild.Name}"));
            }
            else
            {
                var toxicity = _nudeScore.GetChannelTopScores(channel);
                if (toxicity == null || toxicity.IsNullOrWhiteSpace())
                {
                    await Context.ReplyAsync("No values", Color.Red.RawValue);
                    return;
                }

                await Context.ReplyAsync(toxicity);
            }
        }

        [Name("Reason")]
        [Command("reason", RunMode = RunMode.Async)]
        [Summary("Inputs reason for moderation log entry")]
        [Remarks("h.reason 1337 reason")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ApplyReason(int id, [Remainder] string reason)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                var actionCase = await db.ModLogs.FindAsync(id, Context.Guild.Id);
                var updMsg = await Context.Channel.GetMessageAsync(actionCase.MessageId) as IUserMessage;
                if (updMsg == null)
                {
                    await ReplyAndDeleteAsync("Something went wrong, retrying in 5 seconds.",
                        timeout: TimeSpan.FromSeconds(5));
                    var delay = Task.Delay(5000);
                    var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild).ConfigureAwait(false);
                    await Task.WhenAll(delay);
                    if (cfg.LogBan.HasValue)
                        updMsg = await Context.Guild.GetTextChannel(cfg.LogBan.Value)
                            .GetMessageAsync(actionCase.MessageId) as IUserMessage;
                }

                if (updMsg == null)
                {
                    await ReplyAndDeleteAsync("Something went wrong, aborting.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }

                var embed = updMsg.Embeds.FirstOrDefault().ToEmbedBuilder();
                if (embed == null)
                {
                    await ReplyAndDeleteAsync("Something went wrong.", timeout: TimeSpan.FromSeconds(20));
                    return;
                }

                var modField = embed.Fields.FirstOrDefault(x => x.Name == "Moderator");
                var reasonField = embed.Fields.FirstOrDefault(x => x.Name == "Reason");

                if (modField != null) modField.Value = Context.User.Mention;
                if (reasonField != null) reasonField.Value = reason != null ? $"{reason}" : "No Reason Provided";

                await updMsg.ModifyAsync(m => m.Embed = embed.Build());
                actionCase.Response = reason != null ? $"{reason}" : "No Reason Provided";
                actionCase.ModId = Context.User.Id;
                await db.SaveChangesAsync();
            }
        }
    }
}