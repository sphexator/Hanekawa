using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Administration;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Administration
{
    [Name("Self-Assignable Roles")]
    [Description("Self-Assignable commands")]
    [RequireBotChannelPermissions(Permission.SendMessages | Permission.SendEmbeds | Permission.ManageMessages)]
    public class SelfAssignAbleRoles : HanekawaCommandModule
    {
        [Name("I am")]
        [Command("iam", "give")]
        [Description("Assigns a role that's setup as self-assignable")]
        [RequiredChannel]
        public async Task AssignSelfRoleAsync([Remainder] IRole role)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            await Context.Message.TryDeleteMessageAsync();
            var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.GuildId, role.Id);
            if (dbRole == null)
            {
                await ReplyAndDeleteAsync("Couldn't find a self-assignable role with that name", HanaBaseColor.Bad(),
                    TimeSpan.FromSeconds(15));
                return;
            }

            bool addedRole;
            var gUser = Context.Author;
            if (dbRole.Exclusive)
            {
                var roles = await db.SelfAssignAbleRoles
                    .Where(x => x.GuildId == Context.Guild.Id && x.Exclusive)
                    .ToListAsync();
                foreach (var x in roles)
                {
                    if (!Context.Guild.Roles.TryGetValue(x.RoleId, out var exclusiveRole)) return;
                    if (gUser.GetRoles().Values.Contains(exclusiveRole)) await gUser.TryRemoveRoleAsync(exclusiveRole);
                }

                addedRole = await gUser.TryAddRoleAsync(role);
            }
            else
            {
                addedRole = await gUser.TryAddRoleAsync(role);
            }

            if (addedRole)
                await ReplyAndDeleteAsync(
                    $"Added {role.Name} to {Context.Author.Mention}",
                    HanaBaseColor.Ok(), TimeSpan.FromSeconds(10));
            else
                await ReplyAndDeleteAsync(
                    $"Couldn't add {role.Name} to {Context.Author.Mention}, missing permission or role position?",
                    HanaBaseColor.Bad(), TimeSpan.FromSeconds(10));
        }

        [Name("I am not")]
        [Command("iamnot", "iamn")]
        [Description("Removes a role that's setup as self-assignable")]
        [RequiredChannel]
        public async Task RemoveSelfRoleAsync([Remainder] IRole role)
        {
            if (Context.Author.GetRoles().Values.FirstOrDefault(x => x.Id == role.Id) == null) return;

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            await Context.Message.TryDeleteMessageAsync();
            var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.GuildId, role.Id);
            if (dbRole == null)
            {
                await ReplyAndDeleteAsync("Couldn't find a self-assignable role with that name",
                    HanaBaseColor.Bad(), TimeSpan.FromSeconds(15));
                return;
            }

            if (await Context.Author.TryRemoveRoleAsync(role))
                await ReplyAndDeleteAsync(
                    $"Removed {role.Name} from {Context.Author.Mention}", HanaBaseColor.Ok(),
                    TimeSpan.FromSeconds(10));
            else
                await ReplyAndDeleteAsync(
                    $"Couldn't remove {role.Name} from {Context.Author.Mention}, missing permission or role position?",
                    HanaBaseColor.Bad(), TimeSpan.FromSeconds(10));
        }

        [Name("Self-Assignable Roles Admin")]
        [Group("Role")]
        [Description("Commands to manage self-assignable roles")]
        public class AdminRole : SelfAssignAbleRoles, IModuleSetting
        {
            private readonly AutoAssignService _assignService;
            private readonly CacheService _cache;

            public AdminRole(AutoAssignService assignService, CacheService cache)
            {
                _assignService = assignService;
                _cache = cache;
            }

            [Name("List")]
            [Command("List", "rl")]
            [Description("Displays list of self-assignable roles")]
            [RequiredChannel]
            public async Task<DiscordCommandResult> ListSelfAssignAbleRolesAsync()
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var list = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id)
                    .ToListAsync();
                if (list == null || list.Count == 0)
                    return Reply("No self-assignable roles added", HanaBaseColor.Bad());

                var result = new List<string>();
                foreach (var x in list)
                    if (Context.Guild.Roles.TryGetValue(x.RoleId, out var role))
                        result.Add(x.Exclusive ? $"**{role.Name}** (exclusive)" : $"**{role.Name}**");

                return Pages(result.Pagination(_cache.GetColor(Context.GuildId), Context.Guild.GetIconUrl(),
                    $"Self-assignable roles for {Context.Guild.Name}"));
            }

            [Name("Post Reaction Message")]
            [Command("post", "prm")]
            [Description(
                "Posts a message with all self-assignable roles and their respectable reaction emote, adds reactions to message")]
            [RequireAuthorGuildPermissions(Permission.ManageGuild)]
            public async Task PostMessageAsync(ITextChannel channel = null)
            {
                channel ??= Context.Channel as ITextChannel;
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateEntityAsync<ChannelConfig>(Context.GuildId);
                if (!await _assignService.PostAsync(Context, channel, cfg, db))
                {
                    await Reply("There's no self-assignable roles to post!", HanaBaseColor.Bad());
                    return;
                }

                await Reply($"Sent Self-Assignable React Roles to {channel.Mention}!", HanaBaseColor.Ok());
            }

            [Name("Exclusive add")]
            [Command("era")]
            [Description("Adds a role to the list of self-assignable roles")]
            [RequireAuthorGuildPermissions(Permission.ManageGuild)]
            public async Task ExclusiveRole([Remainder] IRole role)
            {
                await AddSelfAssignAbleRoleAsync(Context, role, true);
            }

            [Name("Exclusive add")]
            [Command("era")]
            [Description("Adds a role to the list of self-assignable roles")]
            [Priority(1)]
            [RequireAuthorGuildPermissions(Permission.ManageGuild)]
            public async Task ExclusiveRole(IRole role, LocalCustomEmoji emote)
            {
                await AddSelfAssignAbleRoleAsync(Context, role, true, emote);
            }

            [Name("Add")]
            [Command("add", "ra")]
            [Description("Adds a role to the list of self-assignable roles")]
            [RequireAuthorGuildPermissions(Permission.ManageGuild)]
            public async Task NonExclusiveRole([Remainder] IRole role)
            {
                await AddSelfAssignAbleRoleAsync(Context, role, false);
            }

            [Name("Add")]
            [Command("add", "ra")]
            [Priority(1)]
            [Description("Adds a role to the list of self-assignable roles")]
            [RequireAuthorGuildPermissions(Permission.ManageGuild)]
            public async Task NonExclusiveRole(IRole role, LocalCustomEmoji emote)
            {
                await AddSelfAssignAbleRoleAsync(Context, role, false, emote);
            }

            [Name("Remove")]
            [Command("Remove", "rr")]
            [Description("Removes a role from the list of self-assignable roles")]
            [RequireAuthorGuildPermissions(Permission.ManageGuild)]
            public async Task RemoveSelfAssignAbleRoleAsync([Remainder] IRole role)
            {
                if (!Context.Author.HierarchyCheck(role))
                {
                    await Reply("Can't remove a role that's higher then your highest role.",
                        HanaBaseColor.Bad());
                    return;
                }

                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var roleCheck =
                    await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                        x.GuildId == Context.Guild.Id && x.RoleId == role.Id);
                if (roleCheck == null)
                {
                    await Reply($"There is no self-assignable role by the name {role.Name}",
                        HanaBaseColor.Bad());
                    return;
                }

                db.SelfAssignAbleRoles.Remove(roleCheck);
                await db.SaveChangesAsync();
                await Reply($"Removed {role.Name} as a self-assignable role!", HanaBaseColor.Ok());
            }

            private async Task AddSelfAssignAbleRoleAsync(HanekawaCommandContext context, IRole role, bool exclusive,
                LocalEmoji emote = null)
            {
                if (!context.Author.HierarchyCheck(role))
                {
                    await Reply("Can't add a role that's higher then your highest role.", HanaBaseColor.Bad());
                    return;
                }

                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var roleCheck = await db.SelfAssignAbleRoles.FindAsync(context.Guild.Id, role.Id);
                if (roleCheck != null)
                {
                    switch (exclusive)
                    {
                        case true when !roleCheck.Exclusive:
                            roleCheck.Exclusive = true;
                            await db.SaveChangesAsync();
                            await Reply($"Changed {role.Name} to a exclusive self-assignable role!",
                                HanaBaseColor.Ok());
                            break;
                        case false when roleCheck.Exclusive:
                            roleCheck.Exclusive = false;
                            await db.SaveChangesAsync();
                            await Reply($"Changed {role.Name} to a non-exclusive self-assignable role!",
                                HanaBaseColor.Ok());
                            break;
                        default:
                            await Reply($"{role.Name} is already added as self-assignable",
                                HanaBaseColor.Bad());
                            break;
                    }

                    return;
                }

                var data = new SelfAssignAbleRole
                {
                    GuildId = context.Guild.Id,
                    RoleId = role.Id,
                    EmoteId = emote?.GetId(),
                    Exclusive = exclusive,
                    EmoteMessageFormat = emote?.GetMessageFormat(),
                    EmoteReactFormat = emote?.GetReactionFormat()
                };
                await db.SelfAssignAbleRoles.AddAsync(data);
                await db.SaveChangesAsync();
                if (emote != null)
                {
                    await Reply($"Added {role.Name} as a self-assignable role with emote {emote}!", HanaBaseColor.Ok());
                    await UpdateOrCreateNewEmbeds(context.Guild, data, emote, db);
                }
                else
                {
                    await Reply($"Added {role.Name} as a self-assignable role!", HanaBaseColor.Ok());
                }
            }

            private async ValueTask<bool> UpdateOrCreateNewEmbeds(IGatewayGuild guild, SelfAssignAbleRole update,
                LocalEmoji emote, DbService db)
            {
                var cfg = await db.GetOrCreateEntityAsync<ChannelConfig>(update.GuildId);
                if (!cfg.SelfAssignableChannel.HasValue) return false;
                try
                {
                    if (!guild.Channels.TryGetValue(cfg.SelfAssignableChannel.Value, out var channel)) return false;
                    var textChannel = channel as CachedTextChannel;
                    var reactionRoles = cfg.AssignReactionRoles.FirstOrDefault(x =>
                        x.Reactions.Count < 20 && x.Exclusive == update.Exclusive);
                    IUserMessage message;
                    if (reactionRoles == null)
                    {
                        var strings = new List<string>();
                        if (!guild.Roles.TryGetValue(update.RoleId, out var role)) return false;
                        strings.Add(LocalCustomEmoji.TryParse(update.EmoteMessageFormat, out var result)
                            ? $"{result}{role.Mention}"
                            : $"{role.Mention}");
                        message = await textChannel.SendMessageAsync(
                            new LocalMessage().Create(_assignService.ReactionEmbed(strings, guild.Id)));
                        cfg.AssignReactionRoles.Add(new SelfAssignReactionRole
                        {
                            GuildId = guild.Id,
                            ChannelId = channel.Id,
                            MessageId = message.Id,
                            Exclusive = update.Exclusive,
                            Reactions = new List<string> {emote.GetMessageFormat()}
                        });
                    }
                    else
                    {
                        message = await textChannel.GetOrFetchMessageAsync(reactionRoles.MessageId);
                        var embed = LocalEmbed.FromEmbed(message.Embeds[0]);
                        embed = _assignService.ReactionEmbed(
                            embed.Description.Split("-",
                                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries), guild.Id);
                        await message.ModifyAsync(x => x.Embeds = new[] {embed});
                        reactionRoles.Reactions.Add(emote.GetMessageFormat());
                    }

                    await db.SaveChangesAsync();
                    await message.AddReactionAsync(emote);
                }
                catch
                {
                    return false;
                }

                return true;
            }
        }
    }
}