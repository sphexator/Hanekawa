using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Rest.Default;
using Hanekawa.Bot.Service.Administration.Mute;
using Hanekawa.Bot.Service.Administration.Warning;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Range = Hanekawa.Bot.Commands.TypeReaders.Range;

namespace Hanekawa.Bot.Commands.Modules.Administration
{
    [Name("Administration")]
    [Description("Moderation commands")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public class Administration : HanekawaCommandModule
    {
        private readonly MuteService _mute;
        private readonly WarnService _warn;
        private readonly CacheService _cache;

        public Administration(MuteService mute, WarnService warn, CacheService cache)
        {
            _mute = mute;
            _warn = warn;
            _cache = cache;
        }
        
        [Name("Ban")]
        [Command("ban")]
        [Description("Bans a user")]
        [Priority(1)]
        [RequireAuthorGuildPermissions(Permission.BanMembers | Permission.ManageMessages)]
        public async Task BanAsync(IMember user, [Remainder] string reason = "No reason applied")
        {
            if (user == Context.Author) return;
            await Context.Message.DeleteAsync();
            if (Context.Author == user) return;
            if (!Context.Guild.HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(
                    new LocalMessageBuilder().Create("Cannot ban someone that's higher than me in hierarchy.",
                            HanaBaseColor.Bad()), TimeSpan.FromSeconds(20));
                return;
            }

            if (!Context.Author.HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(
                    new LocalMessageBuilder().Create(
                        $"{Context.Author.Mention}, can't ban someone that's equal or more power than you.",
                        HanaBaseColor.Bad()), TimeSpan.FromSeconds(20));
                return;
            }
            _cache.AddBanCache(Context.GuildId, Context.Author.Id, user.Id);
            await Context.Guild.CreateBanAsync(user.Id, $"{Context.Author.Id} - {reason}", 1);
            await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(
                $"Banned {user.Mention} from {Context.Guild.Name}.",
                HanaBaseColor.Ok()), TimeSpan.FromSeconds(20));
        }
        
        [Name("Ban")]
        [Command("ban")]
        [Description("Bans a user by their ID, doesn't require to be in the server")]
        [RequireBotGuildPermissions(Permission.BanMembers | Permission.ManageMessages)]
        [RequireAuthorGuildPermissions(Permission.BanMembers)]
        public async Task BanAsync(Snowflake userId, [Remainder] string reason = "No reason applied")
        {
            await Context.Message.TryDeleteMessageAsync();
            var user = await Context.Guild.GetOrFetchMemberAsync(userId);
            if (user == null)
            {
                try
                {
                    _cache.AddBanCache(Context.GuildId, Context.Author.Id, userId);
                    await Context.Guild.CreateBanAsync(userId, reason, 1);
                    await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(
                        $"Banned **{userId}** from {Context.Guild.Name}.",
                        HanaBaseColor.Ok()), TimeSpan.FromSeconds(20));
                }
                catch
                {
                    await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(
                        "Couldn't fetch a user by that ID.",
                        Color.Green), TimeSpan.FromSeconds(20));
                }
            }
            else await BanAsync(user, reason);
        }

        [Name("massban")]
        [Command("massban", "mban", "mb")]
        [Description("Bans multiple users by their ID")]
        [RequireBotGuildPermissions(Permission.BanMembers | Permission.ManageMessages)]
        [RequireAuthorGuildPermissions(Permission.BanMembers)]
        public async Task BanAsync(params Snowflake[] ids)
        {
            await Context.Message.TryDeleteMessageAsync();
            await Response($"Attempting to ban {ids.Length} ids...", HanaBaseColor.Orange());
            var banned = 0;
            foreach (var x in ids)
            {
                try
                {
                    _cache.AddBanCache(Context.GuildId, Context.Author.Id, x);
                    await Context.Guild.CreateBanAsync(x, $"Mass ban by {Context.Author} ({Context.Author.Id})", 1);
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    banned++;
                }
                catch
                {
                    banned--;
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            await ReplyAndDeleteAsync(new LocalMessageBuilder().Create($"Successfully banned {banned} accounts!", HanaBaseColor.Ok()),
                TimeSpan.FromSeconds(20));
        }

        [Name("Kick")]
        [Command("kick")]
        [Description("Kicks a user")]
        [RequireBotGuildPermissions(Permission.KickMembers | Permission.ManageMessages)]
        [RequireAuthorGuildPermissions(Permission.KickMembers)]
        public async Task KickAsync(IMember user, [Remainder] string reason = "No reason applied")
        {
            if (user == Context.Author) return;
            await Context.Message.TryDeleteMessageAsync();
            if (!Context.Guild.HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(
                    new LocalMessageBuilder().Create("Cannot kick someone that's higher than me in hierarchy.",
                        Color.Red), TimeSpan.FromSeconds(20));
                return;
            }

            if (!Context.Author.HierarchyCheck(user))
            {
                await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(
                        $"{Context.Author.Mention}, can't kick someone that's equal or more power than you.",
                        Color.Red), TimeSpan.FromSeconds(20));
                return;
            }

            await user.KickAsync(new DefaultRestRequestOptions{Reason = $"{Context.Author} ({Context.Author.Id}) reason: {reason}"});
            await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(
                $"Kicked {user.Mention} from {Context.Guild.Name}.",
                Color.Green), TimeSpan.FromSeconds(20));
        }

        [Name("Prune")]
        [Command("prune")]
        [Description("Prunes X messages, user specific is optional")]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public async Task PruneAsync(int amount = 5, IMember user = null)
        {
            if (amount <= 0) return;
            await Context.Message.TryDeleteMessageAsync();
            var messages = await Context.Channel.FilterMessagesAsync(amount, user);
            try
            {
                if (await Context.Channel.TryDeleteMessagesAsync(messages))
                {
                    await ReplyAndDeleteAsync(
                        new LocalMessageBuilder().Create($"Deleted {amount} messages", Color.Green),
                        TimeSpan.FromSeconds(20));
                }
                else
                {
                    await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(
                        $"Couldn't delete {amount} messages, missing permission?",
                        Color.Red), TimeSpan.FromSeconds(20));
                }
            }
            catch
            {
                await ReplyAndDeleteAsync(
                    new LocalMessageBuilder().Create("Couldn't delete messages, missing permissions?", Color.Red),
                    TimeSpan.FromSeconds(20));
            }
        }

        [Name("Soft Ban")]
        [Command("softban", "sb")]
        [Description("In the last 50 messages, deletes the messages user has sent and mutes")]
        [RequireBotGuildPermissions(Permission.ManageMessages | Permission.ManageRoles | Permission.MuteMembers)]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public async Task SoftBanAsync(IMember user)
        {
            if (Context.Author == user) return;
            await Context.Message.TryDeleteMessageAsync();

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            if (!await _mute.Mute(user, Context.Author, "No Reason Specified (Soft ban)", db))
            {
                await ReplyAndDeleteAsync(new LocalMessageBuilder().Create("Couldn't mute user.", Color.Red),
                    TimeSpan.FromSeconds(20));
            }

            var messages = await Context.Channel.FilterMessagesAsync(50, user);
            await Context.Channel.TryDeleteMessagesAsync(messages);
        }

        [Name("Mute")]
        [Command("mute")]
        [Description("Mutes a user for a duration, specified 1h13m4s or 2342 in minutes with a optional reason")]
        [RequireBotGuildPermissions(Permission.ManageMessages | Permission.ManageRoles | Permission.MuteMembers)]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public async Task MuteAsync(IMember user, TimeSpan? duration = null,
            [Remainder] string reason = "No reason")
        {
            if (user == Context.Author) return;
            await Context.Message.TryDeleteMessageAsync();
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            duration ??= await _mute.GetMuteTimeAsync(user, db);

            var muteRes = await _mute.Mute(user, Context.Author, reason, db, duration.Value);
            if (muteRes)
            {
                await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(
                    $"Muted {user.Mention} for {duration.Value.Humanize(2)}",
                    Color.Green), TimeSpan.FromSeconds(20));
            }
            else
            {
                await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(
                        $"Couldn't mute {user.Mention}, missing permission or role not accessible ?",
                        Color.Red),
                    TimeSpan.FromSeconds(20));
            }
        }

        [Name("UnMute")]
        [Command("unmute")]
        [Description("UnMutes a user")]
        [RequireBotGuildPermissions(Permission.ManageRoles | Permission.MuteMembers)]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public async Task UnMuteAsync(IMember user, [Remainder] string reason = "No reason applied")
        {
            if (user == Context.Author) return;
            await Context.Message.TryDeleteMessageAsync();

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            if (await _mute.UnMuteAsync(user, db))
            {
                await ReplyAndDeleteAsync(new LocalMessageBuilder().Create($"Unmuted {user.Mention}", Color.Green),
                    TimeSpan.FromSeconds(20));
            }
            else
            {
                await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(
                    $"Couldn't unmute {user.Mention}, missing permissions or role not accessible ?",
                    HanaBaseColor.Bad()), TimeSpan.FromSeconds(20));
            }
        }

        [Name("Warn")]
        [Command("warn", "warning")]
        [Description("Warns a user, bot dms them the warning. Warning accessible through warnlog")]
        [RequireBotGuildPermissions(Permission.BanMembers | Permission.KickMembers | Permission.ManageMessages |
                                    Permission.ManageRoles | Permission.MuteMembers)]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public async Task WarnAsync(IMember user, [Remainder] string reason = "No reason")
        {
            if (user == Context.Author) return;
            await Context.Message.TryDeleteMessageAsync();
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            await _warn.Warn(user, Context.Author, reason, WarnReason.Warned, true, db);
            await ReplyAndDeleteAsync(new LocalMessageBuilder().Create($"Warned {user.Mention}", HanaBaseColor.Lime()),
                TimeSpan.FromSeconds(20));
        }

        [Name("Warn Log")]
        [Command("warnlog")]
        [Description("Pulls up warnlog and admin profile of a user.")]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public async Task<DiscordCommandResult> WarnLogAsync(IMember user, WarnLogType type = WarnLogType.Simple)
        {
            await Context.Message.TryDeleteMessageAsync();
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var pages = await _warn.GetWarnLogAsync(user, type, db);
            if (pages.Count == 1) return Reply(pages[0]);
            return Pages(pages.Select(x => (Page) x).ToList());
        }

        [Name("Reason")]
        [Command("reason")]
        [Description("Inputs reason for moderation log entry")]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public async Task ReasonAsync(int id, [Remainder] string reason = "No reason applied")
        {
            if (id <= 0) return;
            await Context.Message.TryDeleteMessageAsync();

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            await ApplyReason(db, id, reason);
            await ReplyAndDeleteAsync(new LocalMessageBuilder().Create($"Updated mod log for {id}", HanaBaseColor.Lime()),
                TimeSpan.FromSeconds(10));
        }

        [Name("Reason")]
        [Command("reason")]
        [Priority(1)]
        [Description("Adds reason to multiple moderation log entries")]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public async Task ReasonAsync(Range range, [Remainder] string reason = "No reason applied")
        {
            if (range.Min <= 0) return;
            await Context.Message.TryDeleteMessageAsync();

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            for (var i = range.Min; i < range.Max; i++)
            {
                await ApplyReason(db, i, reason);
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }

            await ReplyAndDeleteAsync(new LocalMessageBuilder().Create($"Updated mod logs for entries between {range}", HanaBaseColor.Lime()),
                TimeSpan.FromSeconds(10));
        }

        private async Task ApplyReason(DbService db, int id, string reason)
        {
            var modCase = await db.ModLogs.FindAsync(id, Context.Guild.Id);
            await UpdateMessage(modCase, db, reason);

            modCase.Response = reason != null ? $"{reason}" : "No Reason Provided";
            modCase.ModId = Context.Author.Id;
            await db.SaveChangesAsync();
        }

        private async Task UpdateMessage(ModLog modCase, DbService db, string reason)
        {
            var updMsg = await Context.Channel.FetchMessageAsync(modCase.MessageId) as IUserMessage;
            if (updMsg == null)
            {
                await ReplyAndDeleteAsync(
                    new LocalMessageBuilder().Create("Couldn't find the message, retrying in 5 seconds...",
                        HanaBaseColor.Bad()), TimeSpan.FromSeconds(10));
                var delay = Task.Delay(5000);
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild).ConfigureAwait(false);
                await Task.WhenAll(delay);
                if (cfg.LogBan.HasValue)
                {
                    updMsg =
                        await (Context.Guild.GetChannel(cfg.LogBan.Value) as ITextChannel).FetchMessageAsync(
                            modCase.MessageId) as IUserMessage;
                }
            }

            if (updMsg == null)
            {
                await ReplyAndDeleteAsync(
                    new LocalMessageBuilder().Create("Couldn't find the message. aborting...", HanaBaseColor.Bad()),
                    TimeSpan.FromSeconds(10));
                return;
            }

            var embed = LocalEmbedBuilder.FromEmbed(updMsg.Embeds[0]);
            if (embed == null)
            {
                await ReplyAndDeleteAsync(
                    new LocalMessageBuilder().Create("Couldn't find a embed to update...", HanaBaseColor.Bad()),
                    TimeSpan.FromSeconds(20));
                return;
            }

            var modField = embed.Fields.FirstOrDefault(x => x.Name == "Moderator");
            var reasonField = embed.Fields.FirstOrDefault(x => x.Name == "Reason");

            if (modField != null) modField.Value = Context.Author.Mention;
            if (reasonField != null) reasonField.Value = reason != null ? $"{reason}" : "No Reason Provided";

            await updMsg.ModifyAsync(m => m.Embed = embed.Build());
        }
    }
}