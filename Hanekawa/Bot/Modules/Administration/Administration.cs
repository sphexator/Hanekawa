using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Rest;
using Hanekawa.Bot.Services.Administration.Mute;
using Hanekawa.Bot.Services.Administration.Warning;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared;
using Hanekawa.Shared.Command;
using Humanizer;
using Microsoft.SqlServer.Server;
using Qmmands;

namespace Hanekawa.Bot.Modules.Administration
{
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public class Administration : DiscordModuleBase<HanekawaContext>
    {
        private readonly MuteService _mute;
        private readonly WarnService _warn;

        public Administration(MuteService mute, WarnService warn)
        {
            _mute = mute;
            _warn = warn;
        }

        [Name("Ban")]
        [Command("ban")]
        [Description("Bans a user")]
        [Priority(1)]
        [RequireMemberGuildPermissions(Permission.BanMembers, Permission.ManageMessages)]
        [RequireMemberGuildPermissions(Permission.BanMembers)]
        public async Task BanAsync(CachedMember user, [Remainder] string reason = "No reason applied")
        {
            if (user == Context.User) return;
            await Context.Message.TryDeleteMessageAsync();
            if (Context.User == user) return;
            if (!Context.Guild.CurrentMember.HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(
                    null,
                    false,
                    new LocalEmbedBuilder()
                        .Create("Cannot ban someone that's higher than me in hierarchy.",
                            Color.Red).Build(), TimeSpan.FromSeconds(20));
                return;
            }

            if (!Context.Member.HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder().Create(
                        $"{Context.User.Mention}, can't ban someone that's equal or more power than you.",
                        Color.Red).Build(), TimeSpan.FromSeconds(20));
                return;
            }

            await Context.Guild.BanMemberAsync(user.Id, $"{Context.User} ({Context.User.Id}) reason: {reason}", 7);
            await ReplyAndDeleteAsync(null, false, new LocalEmbedBuilder().Create(
                $"Banned {user.Mention} from {Context.Guild.Name}.",
                Color.Green).Build(), TimeSpan.FromSeconds(20));
        }

        [Name("Ban")]
        [Command("ban")]
        [Description("Bans a user by their ID, doesn't require to be in the server")]
        [RequireBotGuildPermissions(Permission.BanMembers, Permission.ManageMessages)]
        [RequireMemberGuildPermissions(Permission.BanMembers)]
        public async Task BanAsync(ulong userId, [Remainder] string reason = "No reason applied")
        {
            await Context.Message.TryDeleteMessageAsync();
            var user = Context.Guild.GetMember(userId);
            if (user != null) await BanAsync(user, reason);
            else
                try
                {
                    await Context.Guild.BanMemberAsync(userId, reason, 7);
                    await ReplyAndDeleteAsync(null, false, new LocalEmbedBuilder().Create(
                        $"Banned {Format.Bold($"{userId}")} from {Context.Guild.Name}.",
                        Color.Green).Build(), TimeSpan.FromSeconds(20));
                }
                catch
                {
                    await ReplyAndDeleteAsync(null, false, new LocalEmbedBuilder().Create(
                        "Couldn't fetch a user by that ID.",
                        Color.Green).Build(), TimeSpan.FromSeconds(20));
                }
        }

        [Name("Kick")]
        [Command("kick")]
        [Description("Kicks a user")]
        [RequireBotGuildPermissions(Permission.KickMembers, Permission.ManageMessages)]
        [RequireMemberGuildPermissions(Permission.KickMembers)]
        public async Task KickAsync(CachedMember user, [Remainder] string reason = "No reason applied")
        {
            if (user == Context.User) return;
            await Context.Message.TryDeleteMessageAsync();
            if (!Context.Guild.CurrentMember.HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(
                    null,
                    false,
                    new LocalEmbedBuilder()
                        .Create("Cannot kick someone that's higher than me in hierarchy.",
                            Color.Red).Build(), TimeSpan.FromSeconds(20));
                return;
            }

            if (!Context.Member.HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder().Create(
                        $"{Context.User.Mention}, can't kick someone that's equal or more power than you.",
                        Color.Red).Build(), TimeSpan.FromSeconds(20));
                return;
            }

            await user.KickAsync(RestRequestOptions.FromReason(reason));
            await ReplyAndDeleteAsync(null, false, new LocalEmbedBuilder().Create(
                $"Kicked {user.Mention} from {Context.Guild.Name}.",
                Color.Green).Build(), TimeSpan.FromSeconds(20));
        }

        [Name("Prune")]
        [Command("prune")]
        [Description("Prunes X messages, user specific is optional")]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task PruneAsync(int amount = 5, CachedMember user = null)
        {
            if (amount <= 0) return;
            await Context.Message.TryDeleteMessageAsync();
            var messages = await Context.Channel.FilterMessagesAsync(amount, user);
            if (await Context.Channel.DeleteMessageAsync(messages))
                await ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder().Create($"Deleted {amount} messages", Color.Green).Build(),
                    TimeSpan.FromSeconds(20));
            else
                await ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder()
                        .Create("Couldn't delete messages, missing permissions?", Color.Red).Build(),
                    TimeSpan.FromSeconds(20));
        }

        [Name("Soft Ban")]
        [Command("softban", "sb")]
        [Description("In the last 50 messages, deletes the messages user has sent and mutes")]
        [RequireBotGuildPermissions(Permission.ManageMessages, Permission.ManageRoles, Permission.MuteMembers)]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task SoftBanAsync(CachedMember user)
        {
            if (Context.Member == user) return;
            await Context.Message.TryDeleteMessageAsync();

            using (var db = new DbService())
            {
                if (!await _mute.Mute(user, db))
                    await ReplyAndDeleteAsync(null, false,
                        new LocalEmbedBuilder().Create("Couldn't mute user. ", Color.Red).Build(),
                        TimeSpan.FromSeconds(20));

                var messages = await Context.Channel.FilterMessagesAsync(50, user);
                await Context.Channel.DeleteMessagesAsync(messages);
            }
        }

        [Name("Mute")]
        [Command("mute")]
        [Description("Mutes a user for a duration, specified 1h13m4s or 2342 in minutes with a optional reason")]
        [RequireBotGuildPermissions(Permission.ManageMessages, Permission.ManageRoles, Permission.MuteMembers)]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task MuteAsync(CachedMember user, TimeSpan? duration = null,
            [Remainder] string reason = "No reason")
        {
            if (user == Context.User) return;
            await Context.Message.TryDeleteMessageAsync();
            if (!duration.HasValue) duration = TimeSpan.FromHours(12);
            using (var db = new DbService())
            {
                var muteRes = await _mute.TimedMute(user, Context.Member, duration.Value, db, reason);
                if (muteRes)
                    await ReplyAndDeleteAsync(null, false,
                        new LocalEmbedBuilder().Create($"Muted {user.Mention} for {duration.Value.Humanize(2)}",
                            Color.Green).Build(), TimeSpan.FromSeconds(20));
                else
                    await ReplyAndDeleteAsync(null, false,
                        new LocalEmbedBuilder()
                            .Create($"Couldn't mute {user.Mention}, missing permission or role not accessible ?",
                                Color.Red).Build(),
                        TimeSpan.FromSeconds(20));
            }
        }

        [Name("Mute")]
        [Command("mute")]
        [Description("Mutes a user")]
        [RequireBotGuildPermissions(Permission.ManageMessages, Permission.ManageRoles, Permission.MuteMembers)]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task MuteAsync(CachedMember user, [Remainder] string reason = "No reason")
        {
            if (user == Context.User) return;
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                var muteRes = await _mute.Mute(user, db);
                if (muteRes)
                    await ReplyAndDeleteAsync(null, false,
                        new LocalEmbedBuilder().Create($"Muted {user.Mention}",
                            Color.Green).Build(), TimeSpan.FromSeconds(20));
                else
                    await ReplyAndDeleteAsync(null, false,
                        new LocalEmbedBuilder()
                            .Create($"Couldn't mute {user.Mention}, missing permission or role not accessible ?",
                                Color.Red).Build(),
                        TimeSpan.FromSeconds(20));
            }
        }

        [Name("UnMute")]
        [Command("unmute")]
        [Description("UnMutes a user")]
        [RequireBotGuildPermissions(Permission.ManageRoles, Permission.MuteMembers)]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task UnMuteAsync(CachedMember user, [Remainder] string reason = "No reason applied")
        {
            if (user == Context.User) return;
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                if (await _mute.UnMuteUser(user, db))
                    await ReplyAndDeleteAsync(null, false,
                        new LocalEmbedBuilder().Create($"Unmuted {user.Mention}", Color.Green).Build(),
                        TimeSpan.FromSeconds(20));
                else
                    await ReplyAndDeleteAsync(null, false,
                        new LocalEmbedBuilder()
                            .Create(
                                $"Couldn't unmute {user.Mention}, missing permissions or role not accessible ?",
                                Color.Red).Build(),
                        TimeSpan.FromSeconds(20));
            }
        }

        [Name("Warn")]
        [Command("warn", "warning")]
        [Description("Warns a user, bot dms them the warning. Warning accessible through warnlog")]
        [RequireBotGuildPermissions(Permission.BanMembers, Permission.KickMembers, Permission.ManageMessages, Permission.ManageRoles, Permission.MuteMembers)]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task WarnAsync(CachedMember user, [Remainder] string reason = "No reason")
        {
            if (user == Context.User) return;
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                await _warn.AddWarn(db, user, Context.Member, reason, WarnReason.Warned, true);
                await ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder().Create($"Warned {user.Mention}", Color.Green).Build(),
                    TimeSpan.FromSeconds(20));
            }
        }

        [Name("Warn Log")]
        [Command("warnlog")]
        [Description("Pulls up warnlog and admin profile of a user.")]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task WarnLogAsync(CachedMember user, WarnLogType type = WarnLogType.Simple)
        {
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                if (type == WarnLogType.Simple)
                {
                    await Context.ReplyAsync(await _warn.GetSimpleWarnlogAsync(user, db));
                }
                else
                {
                    var pages = await _warn.GetFullWarnlogAsync(user, db);
                    await Context.PaginatedReply(pages, user, $"Warn log for {user}");
                }
            }
        }

        [Name("Reason")]
        [Command("reason")]
        [Description("Inputs reason for moderation log entry")]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task ReasonAsync(int id, [Remainder] string reason = "No reason applied")
        {
            if (id <= 0) return;
            await Context.Message.TryDeleteMessageAsync();
            using (var db = new DbService())
            {
                var modCase = await db.ModLogs.FindAsync(id, Context.Guild.Id);
                if (modCase == null)
                {
                    await ReplyAndDeleteAsync(null, false,
                        new LocalEmbedBuilder()
                            .Create("Couldn't find a case with that ID. Sure you wrote the right ID?",
                                Color.Red).Build(), TimeSpan.FromSeconds(20));
                    return;
                }

                var updMsg = await Context.Channel.GetMessageAsync(modCase.MessageId) as IUserMessage;
                if (updMsg == null)
                {
                    await ReplyAndDeleteAsync("Something went wrong, retrying in 5 seconds.",
                        timeout: TimeSpan.FromSeconds(10));
                    var delay = Task.Delay(5000);
                    var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild).ConfigureAwait(false);
                    await Task.WhenAll(delay);
                    if (cfg.LogBan.HasValue)
                        updMsg = await Context.Guild.GetTextChannel(cfg.LogBan.Value)
                            .GetMessageAsync(modCase.MessageId) as IUserMessage;
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
                modCase.Response = reason != null ? $"{reason}" : "No Reason Provided";
                modCase.ModId = Context.User.Id;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder().Create($"Updated mod log for {id}", Color.Green).Build(),
                    TimeSpan.FromSeconds(10));
            }
        }
    }
}